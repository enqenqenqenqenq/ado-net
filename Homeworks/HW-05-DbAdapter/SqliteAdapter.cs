using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite;

namespace HW_05_DbAdapter
{
    /// <summary>
    /// Реализация адаптера для СУБД SQLite
    /// </summary>
    public class SqliteAdapter : IDbAdapter
    {
        private readonly string _connectionString;

        public SqliteAdapter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ProviderName => "SQLite";

        public DbConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public DbParameter CreateParameter(string name, object? value)
        {
            return new SqliteParameter(name, value ?? DBNull.Value);
        }

        public string MapTypeToSql(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short) || underlying == typeof(byte))
                return "INTEGER";

            if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
                return "REAL";

            if (underlying == typeof(bool))
                return "INTEGER";

            if (underlying == typeof(string) || underlying == typeof(DateTime) || underlying == typeof(Guid))
                return "TEXT";

            if (underlying.IsEnum)
                return "INTEGER";

            throw new NotSupportedException($"Тип «{type.Name}» не поддерживается в SQLite адаптере.");
        }

        public string GenerateCreateTableSql(string tableName, IReadOnlyList<ColumnInfo> columns)
        {
            var sb = new StringBuilder($"CREATE TABLE IF NOT EXISTS {tableName} (");

            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var sqlTypeName = MapTypeToSql(col.Type);

                sb.Append($"{col.ColumnName} {sqlTypeName}");

                if (col.IsPrimaryKey)
                {
                    sb.Append(" PRIMARY KEY");
                }

                if (col.IsAutoIncrement)
                {
                    sb.Append(" AUTOINCREMENT");
                }

                if (!col.IsNullable)
                {
                    sb.Append(" NOT NULL");
                }

                if (!string.IsNullOrEmpty(col.CheckConstraint))
                {
                    sb.Append($" CHECK ({col.CheckConstraint})");
                }

                if (i < columns.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(");");
            return sb.ToString();
        }

        public string GenerateInsertSql(string tableName, IReadOnlyList<ColumnInfo> columns, out List<string> parameterNames)
        {
            var insertColumns = columns.Where(c => !c.IsAutoIncrement).ToList();
            parameterNames = new List<string>();

            var sb = new StringBuilder($"INSERT INTO {tableName} (");

            for (int i = 0; i < insertColumns.Count; i++)
            {
                sb.Append(insertColumns[i].ColumnName);
                if (i < insertColumns.Count - 1) sb.Append(", ");
            }

            sb.Append(") VALUES (");

            for (int i = 0; i < insertColumns.Count; i++)
            {
                string pName = $"@p{i}";
                parameterNames.Add(pName);
                sb.Append(pName);
                if (i < insertColumns.Count - 1) sb.Append(", ");
            }

            sb.Append(");");
            return sb.ToString();
        }

        public string GenerateSelectAllSql(string tableName, IReadOnlyList<ColumnInfo> columns)
        {
            var colNames = string.Join(", ", columns.Select(c => c.ColumnName));
            return $"SELECT {colNames} FROM {tableName};";
        }
    }
}
