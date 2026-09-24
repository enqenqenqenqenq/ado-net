namespace HW_03_ColorKeeper
{
    internal class Program
    {
        private static void Main()
        {
            const string connectionString = "Data Source=colors.db;";

            var repository = new SqliteColorRepository(connectionString);
            repository.InitializeDatabase();

            var ui = new ConsoleUI(repository);
            ui.Run();
        }
    }
}
