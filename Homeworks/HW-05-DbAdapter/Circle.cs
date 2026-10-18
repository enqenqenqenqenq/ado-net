namespace HW_05_DbAdapter
{
    /// <summary>
    /// Тестовая модель окружности с атрибутами разметки БД
    /// </summary>
    [Table]
    public class Circle
    {
        [ColumnConfig(IsPrimaryKey = true, IsAutoIncrement = true)]
        public int Id { get; set; }

        [ColumnConfig(CheckConstraint = "Radius > 0")]
        public float Radius { get; set; }

        public string Name { get; set; } = string.Empty;

        public override string ToString() => $"[ID: {Id}] Имя: «{Name}», Радиус: {Radius}";
    }
}
