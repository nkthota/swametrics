using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SWAMetrics.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;


    public class Database
    {
        readonly SqlConnection _connection;
        public string Table { get; set; }

        public Database(string username, string password, string serverUrl, string database)
        {
            _connection = new SqlConnection(string.Format("user id={0};password={1};server={2};database={3};connection timeout=120", username, password, serverUrl, database));
            Table = "";
        }

        public Database()
        {

            // TODO: Complete member initialization
        }

        /*
         * Executes generic sql commands
         * Takes in sql commands to run as is on the db
         * Returns filled DataTable if Select, empty DataTable if not select, null if error
         */
        public DataTable SqlQuery(string query)
        {
            try
            {
                _connection.Open();
                if (query.Length >= 6 && query.Substring(0, 6).ToLower().Equals("select"))
                {
                    SqlDataAdapter da = new SqlDataAdapter(GetCommand(query));
                    DataTable data = new DataTable();
                    da.Fill(data);
                    _connection.Close();
                    return data;
                }
                else
                {
                    GetCommand(query).ExecuteNonQuery();
                    _connection.Close();
                    return new DataTable();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("SQL generic query failed");
                _connection.Close();
                return null;
            }
        }

        /*
         * Reads data from table
         * Takes in comma separated columns to return, and filter/filterValue to use with WHERE
         * sample query given arguments: 
         *      SELECT columns FROM dbAccess.table WHERE filter=filterValue;
         * Returns List of all rows that match the read query or null if error
         */
        public List<string[]> ReadFiltered(string columns, string filter, string filterValue)
        {
            try
            {
                _connection.Open();
                string command = "SELECT " + columns + " FROM " + Table;
                if (!filter.Equals("") && !filterValue.Equals(""))
                {
                    command += " WHERE " + filter + "='" + filterValue + "'";
                }
                command += ";";
                SqlDataReader dataReader = GetCommand(command).ExecuteReader();
                List<string[]> data = new List<string[]>();
                while (dataReader.Read())
                {
                    string[] currentRow = new string[dataReader.FieldCount];
                    for (int i = 0; i < currentRow.Length; i++)
                    {
                        currentRow[i] = dataReader[i].ToString();
                    }
                    data.Add(currentRow);
                }
                _connection.Close();
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("SQL read query failed");
                _connection.Close();
                return null;
            }

        }

        /*
         * Reads data from table
         * Same as readFiltered but returns selected columns from ALL rows
         * Returns List of all rows that match the read query or null if error
         */
        public List<string[]> Read(string columns)
        {
            return ReadFiltered(columns, "", "");
        }

        /*
         * Inserts new row into table
         * Takes in string[] of all column values for new row entry
         * Returns number of rows affected by query, returns 0 if error
         */
        public int Insert(String[] values)
        {
            try
            {
                _connection.Open();
                string command = "INSERT INTO " + Table + " VALUES (";
                for (int i = 0; i < values.Length - 1; i++)
                {
                    command += values[i] + ", ";
                }
                command += values[values.Length - 1] + ");";

                int rowsAffected = GetCommand(command).ExecuteNonQuery();
                _connection.Close();
                return rowsAffected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("SQL insert failed");
                _connection.Close();
                return 0;
            }
        }

        /*
         * Updates row(s) in table
         * Takes in string[] of columns to change, string[] of the values those columns should change to,
         * and a filter/filterValue to use with WHERE to specify which rows to update
         * Returns number of rows affected by query, returns 0 if error
         */
        public int Update(string[] columns, string[] columnValues, string filter, string filterValue)
        {
            try
            {
                _connection.Open();
                string command = "UPDATE " + Table + " SET ";
                for (int i = 0; i < columns.Length - 1; i++)
                {
                    command += columns[i] + "='" + columnValues[i] + "', ";
                }
                command += columns[columns.Length - 1] + "='" + columnValues[columns.Length - 1];
                command += "' WHERE " + filter + "='" + filterValue + "';";

                int rowsAffected = GetCommand(command).ExecuteNonQuery();
                _connection.Close();
                return rowsAffected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("SQL update failed");
                _connection.Close();
                return 0;
            }
        }

        /*
         * Helper method used to create the queries to execute
         * Returns null if error
         */
        private SqlCommand GetCommand(string command)
        {
            try
            {
                return new SqlCommand(command, _connection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

    }
}