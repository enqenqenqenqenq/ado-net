using System.Data.Common;
using System.Text;

namespace HW_05_DbAdapter
{
    /// <summary>
    /// Пример адаптера для Microsoft SQL Server.
    /// Демонстрирует, как легко подключается любая другая СУБД без изменения кода DbManager.
    /// </summary>
    public class SqlServerAdapter : IDbAdapter
    {
        private readonly string _connectionString;

        public SqlServerAdapter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ProviderName => "Microsoft SQL Server";

        public DbConnection CreateConnection()
        {
            // При подключении пакета Microsoft.Data.SqlClient здесь создается:
            // return new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            throw new NotImplementedException("Для работы с реальным MS SQL Server подключите пакет Microsoft.Data.SqlClient.");
        }

        public DbParameter CreateParameter(string name, object? value)
        {
            // return new Microsoft.Data.SqlClient.SqlParameter(name, value ?? DBNull.Value);
            throw new NotImplementedException("Для работы с реальным MS SQL Server подключите пакет Microsoft.Data.SqlClient.");
        }

        public string MapTypeToSql(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (underlying == typeof(int)) return "INT";
            if (underlying == typeof(long)) return "BIGINT";
            if (underlying == typeof(short)) return "SMALLINT";
            if (underlying == typeof(float) || underlying == typeof(double)) return "FLOAT";
            if (underlying == typeof(decimal)) return "DECIMAL(18, 4)";
            if (underlying == typeof(bool)) return "BIT";
            if (underlying == typeof(string)) return "NVARCHAR(MAX)";
            if (underlying == typeof(DateTime)) return "DATETIME2";
            if (underlying == typeof(Guid)) return "UNIQUEIDENTIFIER";

            throw new NotSupportedException($"Тип «{type.Name}» не поддерживается в SQL Server адаптере.");
        }

        public string GenerateCreateTableSql(string tableName, IReadOnlyList<ColumnInfo> columns)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}')");
            sb.AppendLine("BEGIN");
            sb.Append($"    CREATE TABLE [{tableName}] (");

            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                var sqlTypeName = MapTypeToSql(col.Type);

                sb.Append($"[{col.ColumnName}] {sqlTypeName}");

                if (col.IsPrimaryKey)
                {
                    sb.Append(" PRIMARY KEY");
                }

                if (col.IsAutoIncrement)
                {
                    sb.Append(" IDENTITY(1,1)"); // Специфика диалекта SQL Server вместо SQLite AUTOINCREMENT
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

            sb.AppendLine(");");
            sb.AppendLine("END");
            return sb.ToString();
        }

        public string GenerateInsertSql(string tableName, IReadOnlyList<ColumnInfo> columns, out List<string> parameterNames)
        {
            var insertColumns = columns.Where(c => !c.IsAutoIncrement).ToList();
            parameterNames = new List<string>();

            var sb = new StringBuilder($"INSERT INTO [{tableName}] (");
            for (int i = 0; i < insertColumns.Count; i++)
            {
                sb.Append($"[{insertColumns[i].ColumnName}]");
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
            var colNames = string.Join(", ", columns.Select(c => $"[{c.ColumnName}]"));
            return $"SELECT {colNames} FROM [{tableName}];";
        }
    }
}
