namespace HW_03_ColorKeeper
{
    /// <summary>
    /// Контракт репозитория для работы с таблицей цветов через ADO.NET
    /// </summary>
    public interface IColorRepository
    {
        /// <summary>
        /// Возвращает общее количество цветов
        /// </summary>
        int GetTotalCount();

        /// <summary>
        /// Возвращает страницу цветов с пагинацией (LIMIT / OFFSET)
        /// </summary>
        List<Color> GetPage(int pageNumber, int pageSize);

        /// <summary>
        /// Добавляет новый цвет
        /// </summary>
        void Add(string name);

        /// <summary>
        /// Переименовывает цвет по ID
        /// </summary>
        void Rename(int id, string newName);

        /// <summary>
        /// Удаляет цвет по ID
        /// </summary>
        void Delete(int id);

        /// <summary>
        /// Получает название цвета по ID или возвращает null, если не найден
        /// </summary>
        string? GetNameById(int id);
    }
}
