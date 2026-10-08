namespace HW_04_ShapesGrid
{
    internal class Program
    {
        private static void Main()
        {
            const string connectionString = "Data Source=grid_shapes.db;";

            var repository = new SqliteFigureRepository(connectionString);
            repository.InitializeDatabase();

            var ui = new ConsoleUI(repository);
            ui.Run();
        }
    }
}
