using Microsoft.Win32;
using PulsenicsTechAs.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace PulsenicsTechAs.Controllers
{
    public class DbController
    {
        public static List<ResponseGet> GetAllPlotsBasicInfo()
        {
            List<ResponseGet> lstPlot = new List<ResponseGet>();
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                string query = "SELECT * FROM plots";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return lstPlot; // return empty List<>
                        }
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("id"));
                            int polyPower = reader.GetInt32(reader.GetOrdinal("poly_power"));

                            lstPlot.Add(new ResponseGet
                            {
                                Id = id,
                                PolyPower = polyPower,
                                PlotId = id
                            });
                        }
                    }
                }
            }
            return lstPlot;
        }
        public static void DeleteDatapoints(List<Datapoint> datapoints)
        {
            // Delete row based on the row's unique Id, so there is no need to specify the plot_id as argument
            // Conversely, deleting based on comparing floating point numbers is unreliable.
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    if (datapoints.Count > 0)
                    {
                        foreach (Datapoint datapoint in datapoints)
                        {
                            command.CommandText = @"DELETE FROM datapoints WHERE id=@datapointId;";
                            command.Parameters.AddWithValue("@datapointId", datapoint.DatapointId);
                            try
                            {
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    Console.WriteLine($"Datapoint x = {datapoint.XCoor} y = {datapoint.YCoor} does not exist.");
                                }
                                command.Parameters.Clear();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                }
            }
        }
        public static void AddDatapoints(int plotId, List<Datapoint> datapoints)
        {
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    if (datapoints.Count > 0)
                    {
                        foreach (Datapoint datapoint in datapoints)
                        {
                            command.CommandText = @"INSERT INTO datapoints (plot_id, x_coor, y_coor)
                                                  VALUES (@plot_id, @x_coor, @y_coor);";
                            command.Parameters.AddWithValue("@plot_id", plotId);
                            command.Parameters.AddWithValue("@x_coor", datapoint.XCoor);
                            command.Parameters.AddWithValue("@y_coor", datapoint.YCoor);
                            try
                            {
                                int rowsAffected = command.ExecuteNonQuery();
                                command.Parameters.Clear();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        public static Dictionary<int, List<Tuple<int, double, double>>> GetPlotDatapoints()
        {
            // No args implies get all datapoints
            Dictionary<int, List<Tuple<int, double, double>>> allDatapoints = new Dictionary<int, List<Tuple<int, double, double>>>();
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                string query = "SELECT id, plot_id, x_coor, y_coor FROM datapoints";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int key = reader.GetInt32(reader.GetOrdinal("plot_id"));
                            int datapointId = reader.GetInt32(reader.GetOrdinal("id"));
                            double xCoor = reader.GetDouble(reader.GetOrdinal("x_coor"));
                            double yCoor = reader.GetDouble(reader.GetOrdinal("y_coor"));
                            Tuple<int, double, double> datapoint = new Tuple<int, double, double>(datapointId, xCoor, yCoor);

                            if (!allDatapoints.ContainsKey(key))
                            {
                                allDatapoints.Add(key, new List<Tuple<int, double, double>> { datapoint });
                            } 
                            else
                            {
                                allDatapoints[key].Add(datapoint);
                            }
                        }
                    }
                }
            }
            return allDatapoints;
        }
        public static List<Tuple<int, double, double>> GetPlotDatapoints(int plotId)
        {
            // Overloading fxn
            List<Tuple<int, double, double>> datapoints = new List<Tuple<int, double, double>>();
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                string query = $"SELECT id, x_coor, y_coor FROM datapoints WHERE plot_id={plotId}";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int datapointId = reader.GetInt32(reader.GetOrdinal("id"));
                            double xCoor = reader.GetDouble(reader.GetOrdinal("x_coor"));
                            double yCoor = reader.GetDouble(reader.GetOrdinal("y_coor"));
                            Tuple<int, double, double> datapoint = new Tuple<int, double, double>(datapointId, xCoor, yCoor);

                            datapoints.Add(datapoint);
                        }
                    }
                }
            }
            return datapoints;
        }

        public static List<double> GetPlotPolyfit(int plotId, int polyPower)
        {
            List<double> polynomialScalars = new List<double>();
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                string query = $"SELECT * FROM polyfit WHERE plot_id={plotId} AND polyfit_power={polyPower} LIMIT 1";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return polynomialScalars; // return empty List<>
                        }
                        while (reader.Read())
                        {
                            for (int i = 0; i <= polyPower; i++)
                            {
                                string colName = $"a{i}"; // the DB columns are named "a0", "a1"... and ends at "a3"
                                double scalar = reader.GetDouble(reader.GetOrdinal(colName));
                                polynomialScalars.Add(scalar);
                            }
                        }
                    }
                }
            }
            return polynomialScalars;
        }

        public static void AddPolyfit(int plotId, List<double> scalars)
        {
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    if (scalars.Count > 0)
                    {
                        
                        command.CommandText = @"INSERT INTO polyfit (plot_id, polyfit_power, a0, a1, a2, a3)
                                              VALUES (@plot_id, @polyfit_power, @a0, @a1, @a2, @a3);";
                        for (int i = 0; i <= 3; i++)
                        {
                            double? val;
                            if (i < scalars.Count)
                            {
                                val = scalars[i];
                            }
                            else
                            {
                                val = null;
                            }
                            string colName = $"a{i}";
                            command.Parameters.AddWithValue(colName, val);
                        }
                        command.Parameters.AddWithValue("@plot_id", plotId);
                        command.Parameters.AddWithValue("@polyfit_power", scalars.Count - 1);
                        try
                        {
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
        }
        public static void DeletePolyfit(int plotId, List<int> powers)
        {
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    if (powers.Count > 0)
                    {
                        string powersConcatenated = string.Join(", ", powers);
                        command.CommandText = $"DELETE FROM polyfit WHERE plot_id={plotId} AND polyfit_power IN ({powersConcatenated});";
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        
                    }
                }
            }
        }
        public static int AddPlot(int polyPower)
        {
            int lastInsertedRowId;
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"INSERT INTO plots (poly_power) VALUES (@poly_power);";
                    command.Parameters.AddWithValue("@poly_power", polyPower);           
                    try
                    {
                        command.ExecuteNonQuery();
                        lastInsertedRowId = checked((int)connection.LastInsertRowId);
                        command.Parameters.Clear();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            return lastInsertedRowId;
        }
        public static int UpdatePlotPolyfitDisplay(int plotId, int polyPower)
        {
            int lastInsertedRowId;
            string connectionStr = DatabaseInitializer.ConnectionStr();
            using (SQLiteConnection connection = new SQLiteConnection(connectionStr))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"UPDATE plots SET poly_power = @poly_power WHERE id = @id;";
                    command.Parameters.AddWithValue("@poly_power", polyPower);
                    command.Parameters.AddWithValue("@id", plotId);
                    try
                    {
                        command.ExecuteNonQuery();
                        lastInsertedRowId = checked((int)connection.LastInsertRowId);
                        command.Parameters.Clear();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            return lastInsertedRowId;
        }
    }
}