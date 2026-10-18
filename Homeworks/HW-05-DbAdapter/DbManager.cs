using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;

namespace HW_05_DbAdapter
{
    /// <summary>
    /// Универсальный менеджер базы данных (мини-ORM).
    /// Не привязан ни к одной конкретной СУБД — вся специфика делегируется через интерфейс IDbAdapter.
    /// </summary>
    public static class DbManager
    {
        private static IDbAdapter? _adapter;
        private static readonly ConcurrentDictionary<Type, List<ColumnInfo>> _typeCache = new();

        /// <summary>
        /// Текущий установленный адаптер
        /// </summary>
        public static IDbAdapter Adapter => _adapter ?? throw new InvalidOperationException("Адаптер СУБД не установлен. Вызовите DbManager.SetAdapter(...) перед началом работы.");

        /// <summary>
        /// Установка адаптера целевой СУБД
        /// </summary>
        public static void SetAdapter(IDbAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        /// <summary>
        /// Создает таблицы в базе данных для всех классов с атрибутом [Table] в указанной сборке
        /// </summary>
        public static void PopulateTables(Assembly? assembly = null)
        {
            var targetAssembly = assembly ?? Assembly.GetExecutingAssembly();
            var types = targetAssembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<TableAttribute>() != null);

            foreach (var type in types)
            {
                CreateTableIfNotExists(type);
            }
        }

        private static void CreateTableIfNotExists(Type type)
        {
            var columns = GetColumnInfos(type);
            var tableName = GetTableName(type);

            // Формирование SQL delegируется адаптеру выбранной СУБД
            string createTableSql = Adapter.GenerateCreateTableSql(tableName, columns);

            using var connection = Adapter.CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = createTableSql;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Добавляет сущность в базу данных
        /// </summary>
        public static void Add<T>(T entity) where T : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var type = typeof(T);
            var columns = GetColumnInfos(type);
            var tableName = GetTableName(type);

            var insertColumns = columns.Where(c => !c.IsAutoIncrement).ToList();
            string insertSql = Adapter.GenerateInsertSql(tableName, columns, out List<string> paramNames);

            using var connection = Adapter.CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = insertSql;

            for (int i = 0; i < insertColumns.Count; i++)
            {
                var col = insertColumns[i];
                var value = col.Property.GetValue(entity);
                var parameter = Adapter.CreateParameter(paramNames[i], value);
                command.Parameters.Add(parameter);
            }

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Извлекает все записи из таблицы и маппит их в список объектов C#
        /// </summary>
        public static List<T> SelectAll<T>() where T : class, new()
        {
            var type = typeof(T);
            var columns = GetColumnInfos(type);
            var tableName = GetTableName(type);

            string selectSql = Adapter.GenerateSelectAllSql(tableName, columns);
            var result = new List<T>();

            using var connection = Adapter.CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = selectSql;

            using DbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var obj = new T();

                foreach (var col in columns)
                {
                    int ordinal;
                    try
                    {
                        ordinal = reader.GetOrdinal(col.ColumnName);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue;
                    }

                    if (reader.IsDBNull(ordinal))
                    {
                        col.Property.SetValue(obj, null);
                        continue;
                    }

                    object rawValue = reader.GetValue(ordinal);
                    Type targetType = Nullable.GetUnderlyingType(col.Type) ?? col.Type;

                    object convertedValue;
                    if (targetType.IsEnum)
                    {
                        convertedValue = Enum.ToObject(targetType, rawValue);
                    }
                    else if (targetType == typeof(Guid) && rawValue is string strGuid)
                    {
                        convertedValue = Guid.Parse(strGuid);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(rawValue, targetType);
                    }

                    col.Property.SetValue(obj, convertedValue);
                }

                result.Add(obj);
            }

            return result;
        }

        // --- Вспомогательные методы извлечения метаданных ---

        public static List<ColumnInfo> GetColumnInfos(Type type)
        {
            return _typeCache.GetOrAdd(type, t =>
            {
                return t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p =>
                    {
                        var config = p.GetCustomAttribute<ColumnConfigAttribute>();
                        return new ColumnInfo
                        {
                            Property = p,
                            ColumnName = config?.ColumnName ?? GetDefaultColumnName(p),
                            IsPrimaryKey = config?.IsPrimaryKey ?? false,
                            IsAutoIncrement = config?.IsAutoIncrement ?? false,
                            IsNullable = config?.IsNullable ?? (!p.PropertyType.IsValueType || Nullable.GetUnderlyingType(p.PropertyType) != null),
                            CheckConstraint = config?.CheckConstraint
                        };
                    })
                    .ToList();
            });
        }

        public static string GetTableName(Type type)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            if (!string.IsNullOrEmpty(tableAttr?.TableName))
            {
                return tableAttr.TableName;
            }

            return char.ToLower(type.Name[0]) + type.Name.Substring(1);
        }

        private static string GetDefaultColumnName(PropertyInfo prop)
            => char.ToLower(prop.Name[0]) + prop.Name.Substring(1);
    }
}
