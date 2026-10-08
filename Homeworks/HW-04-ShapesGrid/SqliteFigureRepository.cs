using Microsoft.Data.Sqlite;

namespace HW_04_ShapesGrid
{
    /// <summary>
    /// Реализация репозитория фигур на клетчатом поле с использованием ADO.NET и SQLite
    /// </summary>
    public class SqliteFigureRepository : IFigureRepository
    {
        private readonly string _connectionString;

        public SqliteFigureRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        /// <summary>
        /// Создание таблицы и первичное наполнение фигурами
        /// </summary>
        public void InitializeDatabase()
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Figures
                (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL CHECK(name != ''),
                    x INTEGER NOT NULL,
                    y INTEGER NOT NULL,
                    CONSTRAINT UQ_Figure_Position UNIQUE (x, y)
                );
            ";
            command.ExecuteNonQuery();

            // Если поле пустое, добавим 12 стартовых фигур (для демонстрации пагинации по 10 штук и поиска в области)
            command.CommandText = "SELECT COUNT(*) FROM Figures;";
            long count = (long)(command.ExecuteScalar() ?? 0L);

            if (count == 0)
            {
                var sampleFigures = new (string Name, int X, int Y)[]
                {
                    ("Король", 0, 0),
                    ("Ферзь", 1, 0),
                    ("Ладья", 2, 0),
                    ("Конь", 3, 0),
                    ("Слон", 4, 0),
                    ("Пешка-1", 0, 1),
                    ("Пешка-2", 1, 1),
                    ("Пешка-3", 2, 1),
                    ("Пешка-4", 3, 1),
                    ("Пешка-5", 4, 1),
                    ("Дракон", 10, 10),
                    ("Башня", -5, -5)
                };

                using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = "INSERT INTO Figures (name, x, y) VALUES (@name, @x, @y);";

                var pName = insertCommand.CreateParameter();
                pName.ParameterName = "@name";
                insertCommand.Parameters.Add(pName);

                var pX = insertCommand.CreateParameter();
                pX.ParameterName = "@x";
                insertCommand.Parameters.Add(pX);

                var pY = insertCommand.CreateParameter();
                pY.ParameterName = "@y";
                insertCommand.Parameters.Add(pY);

                foreach (var (name, x, y) in sampleFigures)
                {
                    pName.Value = name;
                    pX.Value = x;
                    pY.Value = y;
                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        public int GetTotalCount()
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Figures;";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public List<Figure> GetPage(int pageNumber, int pageSize = 10)
        {
            var figures = new List<Figure>();

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name, x, y
                FROM Figures
                ORDER BY id
                LIMIT @limit OFFSET @offset;
            ";

            command.Parameters.AddWithValue("@limit", pageSize);
            command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                figures.Add(new Figure
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    X = reader.GetInt32(2),
                    Y = reader.GetInt32(3)
                });
            }

            return figures;
        }

        public void PlaceNewFigure(string name, int x, int y)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Название фигуры не может быть пустым.");
            }

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Figures (name, x, y) VALUES (@name, @x, @y);";
            command.Parameters.AddWithValue("@name", name.Trim());
            command.Parameters.AddWithValue("@x", x);
            command.Parameters.AddWithValue("@y", y);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                var existing = GetFigureAt(x, y);
                string existingInfo = existing != null ? $" Занято фигурой «{existing.Name}» (ID: {existing.Id})." : "";
                throw new InvalidOperationException($"Клетка ({x}, {y}) уже занята.{existingInfo}", ex);
            }
        }

        public void MoveFigure(int id, int newX, int newY)
        {
            var figure = GetFigureById(id);
            if (figure == null)
            {
                throw new KeyNotFoundException($"Фигура с ID = {id} не найдена на поле.");
            }

            if (figure.X == newX && figure.Y == newY)
            {
                throw new InvalidOperationException($"Фигура «{figure.Name}» уже находится в клетке ({newX}, {newY}).");
            }

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Figures SET x = @x, y = @y WHERE id = @id;";
            command.Parameters.AddWithValue("@x", newX);
            command.Parameters.AddWithValue("@y", newY);
            command.Parameters.AddWithValue("@id", id);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                var existing = GetFigureAt(newX, newY);
                string existingInfo = existing != null ? $" Занято фигурой «{existing.Name}» (ID: {existing.Id})." : "";
                throw new InvalidOperationException($"Невозможно переместить фигуру: клетка ({newX}, {newY}) уже занята.{existingInfo}", ex);
            }
        }

        public void DeleteFigure(int id)
        {
            using var connection = CreateConnection();
            connection.Open();

            if (GetFigureById(id) == null)
            {
                throw new KeyNotFoundException($"Фигура с ID = {id} не найдена на поле.");
            }

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Figures WHERE id = @id;";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public Figure? GetFigureById(int id)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name, x, y FROM Figures WHERE id = @id;";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Figure
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    X = reader.GetInt32(2),
                    Y = reader.GetInt32(3)
                };
            }

            return null;
        }

        public Figure? GetFigureAt(int x, int y)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name, x, y FROM Figures WHERE x = @x AND y = @y;";
            command.Parameters.AddWithValue("@x", x);
            command.Parameters.AddWithValue("@y", y);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Figure
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    X = reader.GetInt32(2),
                    Y = reader.GetInt32(3)
                };
            }

            return null;
        }

        public List<Figure> GetFiguresInRegion(int startX, int startY, int endX, int endY)
        {
            var area = new RectArea(startX, startY, endX, endY);
            var results = new List<Figure>();

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name, x, y
                FROM Figures
                WHERE x >= @minX AND x <= @maxX AND y >= @minY AND y <= @maxY
                ORDER BY y, x;
            ";

            command.Parameters.AddWithValue("@minX", area.MinX);
            command.Parameters.AddWithValue("@maxX", area.MaxX);
            command.Parameters.AddWithValue("@minY", area.MinY);
            command.Parameters.AddWithValue("@maxY", area.MaxY);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new Figure
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    X = reader.GetInt32(2),
                    Y = reader.GetInt32(3)
                });
            }

            return results;
        }
    }
}
