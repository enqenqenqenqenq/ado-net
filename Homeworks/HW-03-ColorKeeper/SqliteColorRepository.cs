using Microsoft.Data.Sqlite;

namespace HW_03_ColorKeeper
{
    /// <summary>
    /// Реализация репозитория цветов на ADO.NET с СУБД SQLite
    /// </summary>
    public class SqliteColorRepository : IColorRepository
    {
        private readonly string _connectionString;

        public SqliteColorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        /// <summary>
        /// Создание таблицы и начальное наполнение тестовыми данными
        /// </summary>
        public void InitializeDatabase()
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Color
                (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE CHECK(name != '')
                );
            ";
            command.ExecuteNonQuery();

            // Если таблица пустая, заполним несколькими цветами для демонстрации пагинации (> 5 записей)
            command.CommandText = "SELECT COUNT(*) FROM Color;";
            long count = (long)(command.ExecuteScalar() ?? 0L);

            if (count == 0)
            {
                string[] defaultColors = { "Красный", "Зелёный", "Синий", "Жёлтый", "Оранжевый", "Фиолетовый", "Бирюзовый" };
                using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = "INSERT INTO Color (name) VALUES (@name);";
                var param = insertCommand.CreateParameter();
                param.ParameterName = "@name";
                insertCommand.Parameters.Add(param);

                foreach (var colorName in defaultColors)
                {
                    param.Value = colorName;
                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        public int GetTotalCount()
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Color;";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public List<Color> GetPage(int pageNumber, int pageSize)
        {
            var result = new List<Color>();

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name
                FROM Color
                ORDER BY id
                LIMIT @limit OFFSET @offset;
            ";

            command.Parameters.AddWithValue("@limit", pageSize);
            command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Color
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            return result;
        }

        public void Add(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Название цвета не может быть пустым.");
            }

            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Color (name) VALUES (@name);";
            command.Parameters.AddWithValue("@name", name.Trim());

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                throw new InvalidOperationException($"Цвет с названием «{name}» уже существует в базе.", ex);
            }
        }

        public void Rename(int id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Новое название цвета не может быть пустым.");
            }

            using var connection = CreateConnection();
            connection.Open();

            if (!Exists(id, connection))
            {
                throw new KeyNotFoundException($"Цвет с ID = {id} не найден.");
            }

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Color SET name = @name WHERE id = @id;";
            command.Parameters.AddWithValue("@name", newName.Trim());
            command.Parameters.AddWithValue("@id", id);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                throw new InvalidOperationException($"Цвет с названием «{newName}» уже существует в базе.", ex);
            }
        }

        public void Delete(int id)
        {
            using var connection = CreateConnection();
            connection.Open();

            if (!Exists(id, connection))
            {
                throw new KeyNotFoundException($"Цвет с ID = {id} не найден.");
            }

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Color WHERE id = @id;";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public string? GetNameById(int id)
        {
            using var connection = CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM Color WHERE id = @id;";
            command.Parameters.AddWithValue("@id", id);

            var result = command.ExecuteScalar();
            return result?.ToString();
        }

        private bool Exists(int id, SqliteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Color WHERE id = @id;";
            command.Parameters.AddWithValue("@id", id);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
}
