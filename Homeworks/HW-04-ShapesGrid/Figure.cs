namespace HW_04_ShapesGrid
{
    /// <summary>
    /// Доменная модель фигуры на регулярном клетчатом поле
    /// </summary>
    public class Figure
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }

        public override string ToString() => $"[ID: {Id}] \"{Name}\" на позиции ({X}, {Y})";
    }

    /// <summary>
    /// Прямоугольная область на поле
    /// </summary>
    public record RectArea(int StartX, int StartY, int EndX, int EndY)
    {
        public int MinX => Math.Min(StartX, EndX);
        public int MaxX => Math.Max(StartX, EndX);
        public int MinY => Math.Min(StartY, EndY);
        public int MaxY => Math.Max(StartY, EndY);
    }
}
