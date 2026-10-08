namespace HW_04_ShapesGrid
{
    /// <summary>
    /// Контракт доменного репозитория для управления фигурами на сетке
    /// </summary>
    public interface IFigureRepository
    {
        /// <summary>
        /// Возвращает общее количество фигур на поле
        /// </summary>
        int GetTotalCount();

        /// <summary>
        /// Возвращает страницу фигур (по умолчанию 10 штук на страницу)
        /// </summary>
        List<Figure> GetPage(int pageNumber, int pageSize = 10);

        /// <summary>
        /// Устанавливает новую фигуру в указанную клетку поля
        /// </summary>
        void PlaceNewFigure(string name, int x, int y);

        /// <summary>
        /// Перемещает существующую фигуру в другую клетку поля
        /// </summary>
        void MoveFigure(int id, int newX, int newY);

        /// <summary>
        /// Удаляет фигуру с поля по её идентификатору
        /// </summary>
        void DeleteFigure(int id);

        /// <summary>
        /// Находит фигуру по идентификатору
        /// </summary>
        Figure? GetFigureById(int id);

        /// <summary>
        /// Находит фигуру в указанных координатах
        /// </summary>
        Figure? GetFigureAt(int x, int y);

        /// <summary>
        /// Возвращает все фигуры в прямоугольной области (включая границы)
        /// </summary>
        List<Figure> GetFiguresInRegion(int startX, int startY, int endX, int endY);
    }
}
