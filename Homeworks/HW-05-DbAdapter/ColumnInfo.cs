using System.Reflection;

namespace HW_05_DbAdapter
{
    /// <summary>
    /// Структура для кэширования метаданных колонок таблицы
    /// </summary>
    public class ColumnInfo
    {
        public PropertyInfo Property { get; set; } = null!;
        public string ColumnName { get; set; } = string.Empty;
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsNullable { get; set; }
        public string? CheckConstraint { get; set; }
        public Type Type => Property.PropertyType;
    }
}
