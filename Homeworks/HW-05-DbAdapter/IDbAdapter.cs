using System.Data.Common;

namespace HW_05_DbAdapter
{
    /// <summary>
    /// Контракт адаптера к СУБД. Абстрагирует создание подключений, команд, параметров
    /// и специфику SQL-диалекта конкретной базы данных.
    /// </summary>
    public interface IDbAdapter
    {
        /// <summary>
        /// Название адаптера / СУБД (например, "SQLite", "PostgreSQL", "MS SQL Server")
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Создает экземпляр DbConnection для подключения к целевой СУБД
        /// </summary>
        DbConnection CreateConnection();

        /// <summary>
        /// Создает параметр команды для безопасной передачи данных
        /// </summary>
        DbParameter CreateParameter(string name, object? value);

        /// <summary>
        /// Сопоставляет тип данных .NET (C#) с SQL-типом конкретной СУБД
        /// </summary>
        string MapTypeToSql(Type type);

        /// <summary>
        /// Генерирует SQL-скрипт создания таблицы с учетом ограничений (PK, Autoincrement, CHECK, NULL)
        /// </summary>
        string GenerateCreateTableSql(string tableName, IReadOnlyList<ColumnInfo> columns);

        /// <summary>
        /// Генерирует SQL-скрипт вставки записи (INSERT) и возвращает имена параметров
        /// </summary>
        string GenerateInsertSql(string tableName, IReadOnlyList<ColumnInfo> columns, out List<string> parameterNames);

        /// <summary>
        /// Генерирует SQL-скрипт выборки всех записей (SELECT)
        /// </summary>
        string GenerateSelectAllSql(string tableName, IReadOnlyList<ColumnInfo> columns);
    }
}
