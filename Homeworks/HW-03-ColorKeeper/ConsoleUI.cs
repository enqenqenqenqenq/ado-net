namespace HW_03_ColorKeeper
{
    /// <summary>
    /// Консольный интерфейс для управления цветами с поддержкой пагинации
    /// </summary>
    public class ConsoleUI
    {
        private const int PageSize = 5;
        private readonly IColorRepository _repository;
        private int _currentPage = 1;

        public ConsoleUI(IColorRepository repository)
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
                    RenderHeaderAndColors();
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
                        case "т": // Русская раскладка для N
                            NextPage();
                            break;

                        case "p":
                        case "з": // Русская раскладка для P
                            PrevPage();
                            break;

                        case "1":
                            AddColor();
                            break;

                        case "2":
                            RenameColor();
                            break;

                        case "3":
                            DeleteColor();
                            break;

                        case "4":
                            FindColorById();
                            break;

                        case "0":
                            isRunning = false;
                            Console.WriteLine("Работа завершена. До свидания!");
                            break;

                        default:
                            ShowError("Неизвестный пункт меню. Нажмите любую клавишу для повтора...");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private void RenderHeaderAndColors()
        {
            int total = _repository.GetTotalCount();
            int totalPages = total == 0 ? 1 : (int)Math.Ceiling((double)total / PageSize);

            if (_currentPage > totalPages) _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;

            Console.WriteLine("==================================================");
            Console.WriteLine("             УЧЕТ ЦВЕТОВ (ADO.NET)                ");
            Console.WriteLine("==================================================");

            if (total == 0)
            {
                Console.WriteLine("База данных пуста. Зарегистрированных цветов нет.");
            }
            else
            {
                Console.WriteLine($"Всего цветов: {total} | Страница {_currentPage} из {totalPages} (по {PageSize} на стр.):\n");
                var colors = _repository.GetPage(_currentPage, PageSize);
                foreach (var color in colors)
                {
                    Console.WriteLine($"  {color}");
                }

                if (total > PageSize)
                {
                    Console.WriteLine("\n[Пагинация]: введите 'N' (вперед) или 'P' (назад)");
                }
            }
            Console.WriteLine("--------------------------------------------------");
        }

        private void RenderMenu()
        {
            Console.WriteLine("МЕНЮ ОПЕРАЦИЙ:");
            Console.WriteLine("  1. Добавить новый цвет");
            Console.WriteLine("  2. Переименовать цвет по ID");
            Console.WriteLine("  3. Удалить цвет по ID");
            Console.WriteLine("  4. Найти название цвета по ID");
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
                ShowInfo("Вы уже на последней странице.");
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
                ShowInfo("Вы уже на первой странице.");
            }
        }

        private void AddColor()
        {
            Console.WriteLine("\n--- Добавление цвета ---");
            Console.Write("Введите название цвета: ");
            string? name = Console.ReadLine();

            _repository.Add(name ?? string.Empty);
            ShowSuccess($"Цвет «{name}» успешно добавлен!");
        }

        private void RenameColor()
        {
            Console.WriteLine("\n--- Переименование цвета ---");
            Console.Write("Введите ID цвета: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Некорректный ID. Ожидается целое число.");
                return;
            }

            Console.Write("Введите новое название: ");
            string? newName = Console.ReadLine();

            _repository.Rename(id, newName ?? string.Empty);
            ShowSuccess($"Цвет с ID {id} успешно переименован в «{newName}»!");
        }

        private void DeleteColor()
        {
            Console.WriteLine("\n--- Удаление цвета ---");
            Console.Write("Введите ID цвета: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Некорректный ID. Ожидается целое число.");
                return;
            }

            _repository.Delete(id);
            ShowSuccess($"Цвет с ID {id} успешно удален!");
        }

        private void FindColorById()
        {
            Console.WriteLine("\n--- Поиск цвета по ID ---");
            Console.Write("Введите ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowError("Некорректный ID. Ожидается целое число.");
                return;
            }

            string? colorName = _repository.GetNameById(id);
            if (colorName == null)
            {
                ShowInfo($"Цвета с ID = {id} не существует в базе.");
            }
            else
            {
                ShowSuccess($"Цвет с ID {id}: «{colorName}»");
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
