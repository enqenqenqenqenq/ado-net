namespace HW_04_ShapesGrid
{
    /// <summary>
    /// Консольный интерфейс для управления фигурами на бесконечном клетчатом поле
    /// </summary>
    public class ConsoleUI
    {
        private const int PageSize = 10;
        private readonly IFigureRepository _repository;
        private int _currentPage = 1;

        public ConsoleUI(IFigureRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            bool isRunning = true;

            while (isRunning)
            {
                try
                {
                    Console.Clear();
                    RenderHeaderAndFigures();
                    RenderMenu();

                    Console.Write("\nВыберите действие (или N/P для перелистывания): ");
                    string? input = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }

                    switch (input.ToLowerInvariant())
                    {
                        case "n":
                        case "т":
                            NextPage();
                            break;

                        case "p":
                        case "з":
                            PrevPage();
                            break;

                        case "1":
                            PlaceNewFigure();
                            break;

                        case "2":
                            MoveFigure();
                            break;

                        case "3":
                            DeleteFigure();
                            break;

                        case "4":
                            SearchInRegion();
                            break;

                        case "5":
                            FindFigureById();
                            break;

                        case "0":
                            isRunning = false;
                            Console.WriteLine("Программа завершена.");
                            break;

                        default:
                            ShowError("Неизвестный пункт меню. Нажмите Enter...");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private void RenderHeaderAndFigures()
        {
            int total = _repository.GetTotalCount();
            int totalPages = total == 0 ? 1 : (int)Math.Ceiling((double)total / PageSize);

            if (_currentPage > totalPages) _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;

            Console.WriteLine("==================================================================");
            Console.WriteLine("      УЧЕТ ФИГУР НА БЕСКОНЕЧНОМ КЛЕТЧАТОМ ПОЛЕ (SQLite / ADO.NET) ");
            Console.WriteLine("==================================================================");

            if (total == 0)
            {
                Console.WriteLine("Поле пустое. Фигур нет.");
            }
            else
            {
                Console.WriteLine($"Всего фигур: {total} | Страница {_currentPage} из {totalPages} (по {PageSize} шт. на страницу):\n");
                var figures = _repository.GetPage(_currentPage, PageSize);
                foreach (var figure in figures)
                {
                    Console.WriteLine($"  {figure}");
                }

                if (total > PageSize)
                {
                    Console.WriteLine("\n[Пагинация]: введите 'N' (вперед) или 'P' (назад)");
                }
            }
            Console.WriteLine("------------------------------------------------------------------");
        }

        private void RenderMenu()
        {
            Console.WriteLine("ОПЕРАЦИИ С ПОЛЕМ:");
            Console.WriteLine("  1. Поставить новую фигуру в клетку (X, Y)");
            Console.WriteLine("  2. Поставить (переместить) существующую фигуру в другую клетку");
            Console.WriteLine("  3. Удалить фигуру с поля по ID");
            Console.WriteLine("  4. Получить список фигур в прямоугольной области {x1, y1, x2, y2}");
            Console.WriteLine("  5. Найти фигуру по ID");
            Console.WriteLine("  0. Выход");
        }

        private void NextPage()
        {
            int total = _repository.GetTotalCount();
            int totalPages = total == 0 ? 1 : (int)Math.Ceiling((double)total / PageSize);

            if (_currentPage < totalPages)
            {
                _currentPage++;
            }
            else
            {
                ShowInfo("Вы уже на последней странице списка.");
            }
        }

        private void PrevPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
            }
            else
            {
                ShowInfo("Вы уже на первой странице списка.");
            }
        }

        private void PlaceNewFigure()
        {
            Console.WriteLine("\n--- Размещение новой фигуры ---");
            Console.Write("Введите название фигуры (например, Конь, Башня, Стена): ");
            string? name = Console.ReadLine();

            Console.Write("Координата X (целое число): ");
            if (!int.TryParse(Console.ReadLine(), out int x))
            {
                ShowError("Некорректная координата X.");
                return;
            }

            Console.Write("Координата Y (целое число): ");
            if (!int.TryParse(Console.ReadLine(), out int y))
            {
                ShowError("Некорректная координата Y.");
                return;
            }

            _repository.PlaceNewFigure(name ?? string.Empty, x, y);
            ShowSuccess($"Фигура «{name}» успешно размещена в клетке ({x}, {y})!");
        }

        private void MoveFigure()
        {
            Console.WriteLine("\n--- Перемещение фигуры в другую клетку ---");
            Console.Write("Введите ID перемещаемой фигуры: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Некорректный ID.");
                return;
            }

            var current = _repository.GetFigureById(id);
            if (current == null)
            {
                ShowError($"Фигура с ID = {id} не найдена.");
                return;
            }

            Console.WriteLine($"Выбрана фигура: «{current.Name}» на позиции ({current.X}, {current.Y})");
            Console.Write("Введите новую координату X: ");
            if (!int.TryParse(Console.ReadLine(), out int newX))
            {
                ShowError("Некорректная координата X.");
                return;
            }

            Console.Write("Введите новую координату Y: ");
            if (!int.TryParse(Console.ReadLine(), out int newY))
            {
                ShowError("Некорректная координата Y.");
                return;
            }

            _repository.MoveFigure(id, newX, newY);
            ShowSuccess($"Фигура «{current.Name}» (ID: {id}) перемещена из ({current.X}, {current.Y}) в ({newX}, {newY})!");
        }

        private void DeleteFigure()
        {
            Console.WriteLine("\n--- Удаление фигуры с поля ---");
            Console.Write("Введите ID фигуры для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Некорректный ID.");
                return;
            }

            var figure = _repository.GetFigureById(id);
            _repository.DeleteFigure(id);
            ShowSuccess($"Фигура «{figure?.Name ?? id.ToString()}» успешно удалена с поля!");
        }

        private void SearchInRegion()
        {
            Console.WriteLine("\n--- Поиск фигур в прямоугольной области {x1, y1, x2, y2} ---");
            Console.Write("Введите X1 (начало): ");
            if (!int.TryParse(Console.ReadLine(), out int x1)) return;
            Console.Write("Введите Y1 (начало): ");
            if (!int.TryParse(Console.ReadLine(), out int y1)) return;
            Console.Write("Введите X2 (конец): ");
            if (!int.TryParse(Console.ReadLine(), out int x2)) return;
            Console.Write("Введите Y2 (конец): ");
            if (!int.TryParse(Console.ReadLine(), out int y2)) return;

            var area = new RectArea(x1, y1, x2, y2);
            var figures = _repository.GetFiguresInRegion(x1, y1, x2, y2);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nРезультаты для области [{area.MinX}..{area.MaxX}, {area.MinY}..{area.MaxY}]: найдено {figures.Count} фигур.");
            Console.ResetColor();

            if (figures.Count == 0)
            {
                Console.WriteLine("В указанной прямоугольной области фигур нет.");
            }
            else
            {
                foreach (var fig in figures)
                {
                    Console.WriteLine($"  {fig}");
                }

                // Визуализация сетки, если область не слишком огромная
                int width = area.MaxX - area.MinX + 1;
                int height = area.MaxY - area.MinY + 1;
                if (width <= 20 && height <= 20)
                {
                    Console.WriteLine("\nМини-карта области ('.' - пусто, '[ID]' - фигура):");
                    var lookup = figures.ToDictionary(f => (f.X, f.Y));

                    for (int y = area.MinY; y <= area.MaxY; y++)
                    {
                        Console.Write($"Y={y,3} | ");
                        for (int x = area.MinX; x <= area.MaxX; x++)
                        {
                            if (lookup.TryGetValue((x, y), out var f))
                            {
                                Console.Write($"[{f.Id}] ");
                            }
                            else
                            {
                                Console.Write(" .  ");
                            }
                        }
                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        private void FindFigureById()
        {
            Console.WriteLine("\n--- Поиск фигуры по ID ---");
            Console.Write("Введите ID фигуры: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Некорректный ID.");
                return;
            }

            var figure = _repository.GetFigureById(id);
            if (figure == null)
            {
                ShowInfo($"Фигура с ID = {id} не найдена на поле.");
            }
            else
            {
                ShowSuccess($"Найдена фигура: {figure}");
            }
        }

        private void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[УСПЕХ]: {message}");
            Console.ResetColor();
            Console.WriteLine("Нажмите Enter для продолжения...");
            Console.ReadLine();
        }

        private void ShowInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[ИНФО]: {message}");
            Console.ResetColor();
            Console.WriteLine("Нажмите Enter для продолжения...");
            Console.ReadLine();
        }

        private void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ОШИБКА]: {message}");
            Console.ResetColor();
            Console.WriteLine("Нажмите Enter для продолжения...");
            Console.ReadLine();
        }
    }
}
