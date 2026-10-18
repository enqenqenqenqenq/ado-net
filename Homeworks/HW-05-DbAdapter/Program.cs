namespace HW_05_DbAdapter
{
    internal class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("==========================================================");
            Console.WriteLine("    ДЕМОНСТРАЦИЯ УНИВЕРСАЛЬНОГО АДАПТЕРА СУБД (ADO.NET)   ");
            Console.WriteLine("==========================================================\n");

            // 1. Инициализируем SQLite адаптер
            const string sqliteConnStr = "Data Source=demo_adapter.db;";
            var sqliteAdapter = new SqliteAdapter(sqliteConnStr);

            DbManager.SetAdapter(sqliteAdapter);
            Console.WriteLine($"[1] Установлен адаптер: {DbManager.Adapter.ProviderName}");

            // 2. Создаем таблицы на основе атрибутов [Table]
            Console.WriteLine("[2] Создание таблиц через адаптер...");
            DbManager.PopulateTables();
            Console.WriteLine("    Таблица для класса Circle успешно инициализирована.\n");

            // 3. Добавляем тестовые объекты
            Console.WriteLine("[3] Вставка объектов в базу данных SQLite:");
            var c1 = new Circle { Radius = 10.5f, Name = "Кругляш" };
            var c2 = new Circle { Radius = 42.0f, Name = "Колесо" };
            var c3 = new Circle { Radius = 100.0f, Name = "Солнце" };

            DbManager.Add(c1);
            DbManager.Add(c2);
            DbManager.Add(c3);
            Console.WriteLine("    Успешно добавлены 3 объекта Circle.\n");

            // 4. Извлекаем все объекты через адаптер
            Console.WriteLine("[4] Чтение всех объектов из базы данных (SelectAll<Circle>):");
            List<Circle> circles = DbManager.SelectAll<Circle>();
            foreach (var circle in circles)
            {
                Console.WriteLine($"    {circle}");
            }

            // 5. Демонстрация адаптера MS SQL Server на тех же моделях
            Console.WriteLine("\n[5] Демонстрация генерации диалекта для другой СУБД (MS SQL Server):");
            var sqlServerAdapter = new SqlServerAdapter("Server=localhost;Database=Demo;Trusted_Connection=True;");
            var columns = DbManager.GetColumnInfos(typeof(Circle));
            string tableName = DbManager.GetTableName(typeof(Circle));

            Console.WriteLine("--- DDL для SQLite: ---");
            Console.WriteLine(sqliteAdapter.GenerateCreateTableSql(tableName, columns));

            Console.WriteLine("\n--- DDL для MS SQL Server (из того же класса Circle): ---");
            Console.WriteLine(sqlServerAdapter.GenerateCreateTableSql(tableName, columns));

            // 6. Проверка обработки исключений (нарушение CHECK constraint Radius > 0)
            Console.WriteLine("[6] Проверка обработки исключения (CHECK constraint Radius > 0):");
            try
            {
                var invalidCircle = new Circle { Radius = -5.0f, Name = "Неправильный круг" };
                DbManager.Add(invalidCircle);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    [УСПЕШНО ПЕРЕХВАЧЕНО]: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nВсе тесты завершены успешно.");
        }
    }
}
