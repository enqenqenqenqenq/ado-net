namespace HW_03_ColorKeeper
{
    /// <summary>
    /// Модель цвета в базе данных
    /// </summary>
    public class Color
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => $"[ID: {Id}] {Name}";
    }
}
