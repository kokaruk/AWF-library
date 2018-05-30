using System;
using System.Configuration;
using System.Data;
using Npgsql;

namespace AWFLib.DBLayer
{
    static class AWFPostgresDataLayer
    {
        internal static string insertSelectTransaction(string sql, string[] parameterNames, string[] parameterVals)
        {
            using (NpgsqlConnection connection = GetDbConnection())
            {
                //Create a new transaction
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection, transaction))
                        {
                            FillParameters(command, parameterNames, parameterVals);
                            object result = command.ExecuteScalar();
                            //No exceptions encountered
                            transaction.Commit();
                            return Convert.ToString(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        //Transaction rolled back to the original state
                        Console.WriteLine("{0} Exception", ex);
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal static int updateTransaction(string sql, string[] parameterNames, string[] parameterVals)
        {
            using (NpgsqlConnection connection = GetDbConnection())
            {
                //Create a new transaction
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection, transaction))
                        {
                            FillParameters(command, parameterNames, parameterVals);
                            int rowSaffected = command.ExecuteNonQuery();
                            //No exceptions encountered
                            transaction.Commit();
                            return rowSaffected;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Transaction rolled back to the original state
                        Console.WriteLine("{0} Exception", ex);
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal static DataTable GetDataTable(string sql, string[] parameterNames, string[] parameterVals)
        {
            using (NpgsqlConnection connection = GetDbConnection())
            {
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
                {
                    DataTable table = new DataTable();
                    FillParameters(da.SelectCommand, parameterNames, parameterVals);
                    da.Fill(table);
                    return table;
                }
            }
        }

        internal static DataTable GetDataTable(string sql)
        {
            using (NpgsqlConnection connection = GetDbConnection())
            {
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
                {
                    DataTable table = new DataTable();
                    da.Fill(table);
                    return table;
                }
            }
        }

        internal static string SelectScalar(string sql, string[] parameterNames, string[] parameterVals)
        {
            using (NpgsqlConnection connection = GetDbConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    FillParameters(command, parameterNames, parameterVals);
                    return Convert.ToString(command.ExecuteScalar());
                }
            }
        }

        internal static string SelectScalar(string sql)
        {
            using (NpgsqlConnection connection = GetDbConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    return Convert.ToString(command.ExecuteScalar());
                }
            }
        }

        internal static int ExecuteNonQuery(string sql, string[] parameterNames, string[] parameterVals)
        {
            using (NpgsqlConnection connection = GetDbConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    FillParameters(command, parameterNames, parameterVals);
                    return command.ExecuteNonQuery();
                }
            }
        }

        static void FillParameters(NpgsqlCommand command, string[] parameterNames, string[] parameterVals)
        {
            if (parameterNames != null)
            {
                for (int i = 0; i <= parameterNames.Length - 1; i++)
                {
                    command.Parameters.AddWithValue(parameterNames[i], parameterVals[i]);
                }
            }
        }

        static NpgsqlConnection GetDbConnection()
        {
            var conString = ConfigurationManager.ConnectionStrings["Prinlut"].ConnectionString;
            // read connectionstring from ini file
            //Dim conString As String = getConnectionString()
            NpgsqlConnection connection = new NpgsqlConnection(conString);
            connection.Open();
            return connection;
        }
    }
}
