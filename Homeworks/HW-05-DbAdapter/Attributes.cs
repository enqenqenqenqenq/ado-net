namespace HW_05_DbAdapter
{
    /// <summary>
    /// Атрибут для пометки сущностей, сохраняемых в базу данных в виде таблиц
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string? TableName { get; set; }

        public TableAttribute(string? tableName = null)
        {
            TableName = tableName;
        }
    }

    /// <summary>
    /// Атрибут для настройки параметров колонки таблицы
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnConfigAttribute : Attribute
    {
        public string? ColumnName { get; set; }
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsAutoIncrement { get; set; } = false;
        public bool IsNullable { get; set; } = false;
        public string? CheckConstraint { get; set; } // Например, "Radius > 0"
    }
}
