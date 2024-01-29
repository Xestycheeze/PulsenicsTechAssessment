using System.Data.SQLite;
using System;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

public static class DatabaseInitializer
{
    private static string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static string _connectionStr = $"Data Source={projectDirectory}\\Files\\LibraryManagementSystem.db;Version=3";

    public static string ConnectionStr()
    {
        return _connectionStr;
    }
    public static void InitializeDatabase()
    {
        if (!File.Exists($"{projectDirectory}\\Files\\LibraryManagementSystem.db"))
        {
            SQLiteConnection.CreateFile($"{projectDirectory}\\Files\\LibraryManagementSystem.db");

            using (var connection = new SQLiteConnection(ConnectionStr()))
            {
                connection.Open();

                // poly_power = 0 tells the frontend to not display a curve
                string createPlotsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS plots (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        poly_power INTEGER CHECK (poly_power BETWEEN 0 AND 3)
                    );";

                string createDatapointsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS datapoints (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        plot_id INTEGER REFERENCES plots(id),
                        x_coor NUMERIC NOT NULL,
                        y_coor NUMERIC NOT NULL
                    );";

                //  any curve of best fit must have the primary slope and bias
                string createPolyfitTableQuery = @"
                    CREATE TABLE IF NOT EXISTS polyfit (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        plot_id INTEGER REFERENCES plots(id),
                        polyfit_power INTEGER CHECK (polyfit_power BETWEEN 1 AND 3),
                        a0 NUMERIC NOT NULL,
                        a1 NUMERIC NOT NULL,
                        a2 NUMERIC NULL,
                        a3 NUMERIC NULL
                    );";
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = createPlotsTableQuery;
                    command.ExecuteNonQuery();

                    command.CommandText = createDatapointsTableQuery; 
                    command.ExecuteNonQuery();

                    command.CommandText = createPolyfitTableQuery;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
