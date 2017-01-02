using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Kragle.Properties;
using Npgsql;
using NpgsqlTypes;


namespace Kragle
{
    /// <summary>
    ///     Database interface.
    /// </summary>
    internal class Database
    {
        private readonly NpgsqlConnection _conn;


        /// <summary>
        ///     Establishes a new database connection.
        /// </summary>
        public Database(string server, int port, string username, string password, string database)
        {
            CreateDatabase(server, port, username, password, database);

            _conn = CreateConnection(server, port, username, password, database);
            _conn.Open();
        }


        /// <summary>
        ///     Resets the given database. That is, the database is dropped and then created again, without tables.
        /// </summary>
        /// <param name="server">the address of the server</param>
        /// <param name="port">the port of the server</param>
        /// <param name="username">the username of the user to connect as</param>
        /// <param name="password">the password of the user</param>
        /// <param name="database">the name of the database</param>
        public static void Reset(string server, int port, string username, string password, string database)
        {
            const string dropQuery = "DROP DATABASE IF EXISTS {0};";

            using (NpgsqlConnection conn = CreateConnection(server, port, username, password))
            {
                conn.Open();

                try
                {
                    new NpgsqlCommand(string.Format(dropQuery, database), conn).ExecuteNonQuery();
                }
                catch (NpgsqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            CreateDatabase(server, port, username, password, database);
        }

        /// <summary>
        ///     Executes the given command and returns the dataset returned from the query. This method is useful for retrieving
        ///     data from prepared queries.
        /// </summary>
        /// <param name="cmd">the command to execute</param>
        /// <returns>the dataset returned from the query</returns>
        public static DataTable ExecuteReaderQuery(DbCommand cmd)
        {
            DbDataReader reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            return dt;
        }

        /// <summary>
        ///     Adds a parameter-value pair to a prepared query.
        /// </summary>
        /// <param name="cmd">a prepared query</param>
        /// <param name="parameter">the name of the parameter</param>
        /// <param name="type">the parameter's data type</param>
        /// <returns>this Database instance</returns>
        public static void PrepareParameter(DbCommand cmd, string parameter, NpgsqlDbType type)
        {
            cmd.Parameters.Add(new NpgsqlParameter(parameter, type));
        }

        /// <summary>
        /// Sets the value for a prepared command's parameter.
        /// </summary>
        /// <param name="cmd">a database command</param>
        /// <param name="parameter">the parameter name</param>
        /// <param name="value">the parameter value</param>
        public static void SetParameter(DbCommand cmd, string parameter, object value)
        {
            NpgsqlCommand npgsqlCmd = cmd as NpgsqlCommand;
            Debug.Assert(npgsqlCmd != null, "cmd is an NpgsqlCommand");
            npgsqlCmd.Parameters[parameter].Value = value;
        }

        /// <summary>
        ///     Creates a command object encapsulating an SQL query.
        /// </summary>
        /// <param name="sql">the SQL</param>
        /// <returns>a command object</returns>
        public NpgsqlCommand CreateQuery(string sql)
        {
            return new NpgsqlCommand(sql, _conn);
        }

        /// <summary>
        ///     Executes the given SQL and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">the SQL to execute</param>
        /// <returns>the number of affected rows</returns>
        public int ExecuteQuery(string sql)
        {
            return CreateQuery(sql).ExecuteNonQuery();
        }

        /// <summary>
        ///     Executes the given SQL and returns the dataset returned from the query.
        /// </summary>
        /// <param name="sql">the SQL to execute</param>
        /// <returns>the dataset returned from the query</returns>
        public DataTable ExecuteReaderQuery(string sql)
        {
            return ExecuteReaderQuery(CreateQuery(sql));
        }


        /// <summary>
        ///     Creates a connection to a PostgreSQL server, but does not open it.
        /// </summary>
        /// <param name="server">the address of the server</param>
        /// <param name="port">the port of the server</param>
        /// <param name="username">the username of the user to create the database as</param>
        /// <param name="password">the password of the user</param>
        /// <returns>a connection to a PostgreSQL server</returns>
        private static NpgsqlConnection CreateConnection(string server, int port, string username, string password)
        {
            const string connString = "Server={0};Port={1};Username={2};Password={3};";

            return new NpgsqlConnection(string.Format(connString, server, port, username, password));
        }

        /// <summary>
        ///     Creates a connection to a PostgreSQL database, but does not open it.
        /// </summary>
        /// <param name="server">the address of the server</param>
        /// <param name="port">the port of the server</param>
        /// <param name="username">the username of the user to connect as</param>
        /// <param name="password">the password of the user</param>
        /// <param name="database">the name of the database</param>
        /// <returns>a connection to a PostgreSQL database</returns>
        private static NpgsqlConnection CreateConnection(string server, int port, string username, string password,
            string database)
        {
            const string connString = "Server={0};Port={1};Username={2};Password={3};Database={4};";

            return new NpgsqlConnection(string.Format(connString, server, port, username, password, database));
        }

        /// <summary>
        ///     Creates a database if it does not already exist, and populates it with tables if they do not exist.
        /// </summary>
        /// <param name="server">the address of the server</param>
        /// <param name="port">the port of the server</param>
        /// <param name="username">the username of the user to create the database as</param>
        /// <param name="password">the password of the user</param>
        /// <param name="database">the name of the database</param>
        private static void CreateDatabase(string server, int port, string username, string password, string database)
        {
            const string existsQuery = "SELECT 1 AS result FROM pg_database WHERE datname='{0}';";
            const string createQuery = "CREATE DATABASE {0};";

            // Create database (if it does not exist)
            using (NpgsqlConnection conn = CreateConnection(server, port, username, password))
            {
                conn.Open();

                DataTable data = ExecuteReaderQuery(new NpgsqlCommand(string.Format(existsQuery, database), conn));
                if (data.Rows.Count == 0)
                {
                    new NpgsqlCommand(string.Format(createQuery, database), conn).ExecuteNonQuery();
                }
            }

            // Create table (if they do not exist)
            using (NpgsqlConnection conn = CreateConnection(server, port, username, password, database))
            {
                conn.Open();

                new NpgsqlCommand(Resources.db_create, conn).ExecuteNonQuery();
            }
        }
    }
}
