using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Kragle.Properties;
using Npgsql;
using NpgsqlTypes;


namespace Kragle
{
    /// <summary>
    ///     Database interface.
    /// </summary>
    internal class Database : IDisposable
    {
        private readonly NpgsqlConnection _conn;


        /// <summary>
        ///     Establishes a new database connection.
        /// </summary>
        public Database(string server, int port, string username, string password, string database)
        {
            Initialize(server, port, username, password, database);

            _conn = CreateConnection(server, port, username, password, database);
            _conn.Open();
        }


        /// <summary>
        ///     Returns information on the columns in the given table.
        /// </summary>
        /// <param name="table">the table name</param>
        /// <returns></returns>
        public IDictionary<string, Column> this[string table] => Properties.Tables[table];

        /// <summary>
        ///     Disposes this Database.
        /// </summary>
        public void Dispose()
        {
            _conn.Dispose();
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

                using (Query query = new Query(string.Format(dropQuery, database), conn))
                {
                    query.ExecuteNonQuery();
                }
            }

            Initialize(server, port, username, password, database);
        }

        /// <summary>
        ///     Constructs a new Query.
        /// </summary>
        /// <param name="sql">the SQL</param>
        /// <returns>a new Query</returns>
        public Query CreateQuery(string sql)
        {
            return new Query(sql, _conn);
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
        private static void Initialize(string server, int port, string username, string password, string database)
        {
            const string existsQuery = "SELECT 1 AS result FROM pg_database WHERE datname='{0}';";
            const string createQuery = "CREATE DATABASE {0};";

            // Create database (if it does not exist)
            using (NpgsqlConnection conn = CreateConnection(server, port, username, password))
            {
                conn.Open();

                // Check if exists
                bool databaseExists;
                using (Query query = new Query(string.Format(existsQuery, database), conn))
                {
                    DataTable table = query.ExecuteQuery();
                    databaseExists = table.Rows.Count != 0;
                }

                // Create if not exists
                if (!databaseExists)
                {
                    using (Query query = new Query(string.Format(createQuery, database), conn))
                    {
                        query.ExecuteNonQuery();
                    }
                }
            }

            // Create tables (SQL checks that tables do not exist)
            using (NpgsqlConnection conn = CreateConnection(server, port, username, password, database))
            {
                conn.Open();

                using (Query query = new Query(Resources.db_create, conn))
                {
                    query.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        ///     Contains properties of tables and columns.
        /// </summary>
        private static class Properties
        {
            public static readonly IDictionary<string, IDictionary<string, Column>> Tables;


            /// <summary>
            ///     Static initialiser for tables.
            /// </summary>
            static Properties()
            {
                Tables = new Dictionary<string, IDictionary<string, Column>>
                {
                    ["user"] = new Dictionary<string, Column>
                    {
                        ["_"] = new Column("scratch_user", NpgsqlDbType.Integer),
                        ["id"] = new Column("user_id", NpgsqlDbType.Integer),
                        ["name"] = new Column("username", NpgsqlDbType.Varchar),
                        ["joinDate"] = new Column("join_date", NpgsqlDbType.Bigint),
                        ["country"] = new Column("country", NpgsqlDbType.Varchar)
                    },
                    ["userProject"] = new Dictionary<string, Column>
                    {
                        ["_"] = new Column("user_project", NpgsqlDbType.Integer),
                        ["userId"] = new Column("user_id", NpgsqlDbType.Integer),
                        ["projectId"] = new Column("project_id", NpgsqlDbType.Integer)
                    },
                    ["project"] = new Dictionary<string, Column>
                    {
                        ["_"] = new Column("project", NpgsqlDbType.Integer),
                        ["id"] = new Column("project_id", NpgsqlDbType.Integer),
                        ["title"] = new Column("title", NpgsqlDbType.Varchar),
                        ["createDate"] = new Column("create_date", NpgsqlDbType.Bigint),
                        ["modifyDate"] = new Column("modify_date", NpgsqlDbType.Bigint),
                        ["shareDate"] = new Column("share_date", NpgsqlDbType.Bigint),
                        ["viewCount"] = new Column("view_count", NpgsqlDbType.Integer),
                        ["loveCount"] = new Column("love_count", NpgsqlDbType.Integer),
                        ["favoriteCount"] = new Column("favorite_count", NpgsqlDbType.Integer),
                        ["commentCount"] = new Column("comment_count", NpgsqlDbType.Integer),
                        ["remixParentId"] = new Column("remix_parent_id", NpgsqlDbType.Integer),
                        ["remixRootId"] = new Column("remix_parent_root", NpgsqlDbType.Integer)
                    },
                    ["projectCode"] = new Dictionary<string, Column>
                    {
                        ["_"] = new Column("project_code", NpgsqlDbType.Integer),
                        ["projectId"] = new Column("project_id", NpgsqlDbType.Integer),
                        ["fetchDate"] = new Column("fetch_date", NpgsqlDbType.Bigint),
                        ["code"] = new Column("code", NpgsqlDbType.Text)
                    }
                };
            }
        }
    }


    /// <summary>
    ///     A query to be executed on a database.
    /// </summary>
    internal class Query : IDisposable
    {
        private readonly NpgsqlCommand _command;


        /// <summary>
        ///     Constructs a new Query object.
        /// </summary>
        /// <param name="command">the encapsulated command</param>
        public Query(NpgsqlCommand command)
        {
            _command = command;
        }

        /// <summary>
        ///     Constructs a new Query object.
        /// </summary>
        /// <param name="sql">the SQL query</param>
        /// <param name="conn">the connection on which the Query is executed</param>
        public Query(string sql, NpgsqlConnection conn)
        {
            _command = new NpgsqlCommand(sql, conn);
        }


        /// <summary>
        ///     Disposes this Query.
        /// </summary>
        public void Dispose()
        {
            _command.Dispose();
        }


        /// <summary>
        ///     Prepares this Query without any parameters.
        /// </summary>
        public void Prepare()
        {
            _command.Prepare();
        }

        /// <summary>
        ///     Prepares this Query with the given parameters.
        /// </summary>
        /// <param name="format">true if the query should first be formatted</param>
        /// <param name="offset">the number of leading parameters that are for formatting and not for preparing</param>
        /// <param name="parameters">an enumerable of (name, type) pairs</param>
        public void Prepare(bool format = false, int offset = 0, params Tuple<string, NpgsqlDbType>[] parameters)
        {
            if (format)
            {
                Format(parameters.Select(parameter => (object) parameter.Item1).ToArray());
            }

            foreach (Tuple<string, NpgsqlDbType> parameter in parameters.Skip(offset))
            {
                PrepareParameter(parameter.Item1, parameter.Item2);
            }

            _command.Prepare();
        }

        /// <summary>
        ///     Prepares this Query with the given parameters.
        /// </summary>
        /// <param name="format">true if the query should first be formatted</param>
        /// <param name="offset">the number of leading parameters that are for formatting and not for preparing</param>
        /// <param name="parameters">an enumerable of columns; the name of each column is used as the name of the parameter</param>
        public void Prepare(bool format = false, int offset = 0, params Column[] parameters)
        {
            if (format)
            {
                Format(parameters.Select(parameter => (object) parameter.Name).ToArray());
            }

            foreach (Column parameter in parameters.Skip(offset))
            {
                PrepareParameter(parameter.Name, parameter.Type);
            }

            _command.Prepare();
        }

        /// <summary>
        ///     Prepares this Query with the given parameters.
        /// </summary>
        /// <param name="format">true if the query should first be formatted</param>
        /// <param name="offset">the number of leading parameters that are for formatting and not for preparing</param>
        /// <param name="parameters">
        ///     an enumerable of (name, column) pairs; the name in the outer pair is used rather than the name
        ///     inside the Column object
        /// </param>
        public void Prepare(bool format = false, int offset = 0, params Tuple<string, Column>[] parameters)
        {
            if (format)
            {
                Format(parameters.Select(parameter => (object) parameter.Item1).ToArray());
            }

            foreach (Tuple<string, Column> parameter in parameters.Skip(offset))
            {
                PrepareParameter(parameter.Item1, parameter.Item2.Type);
            }
        }


        /// <summary>
        ///     Sets the parameters for this Query.
        /// </summary>
        /// <param name="parameters">an enumerable of (name, value) pairs</param>
        public void Set(params Tuple<string, object>[] parameters)
        {
            foreach (Tuple<string, object> parameter in parameters)
            {
                SetParameter(parameter.Item1, parameter.Item2);
            }
        }

        /// <summary>
        ///     Sets the parameters for this Query.
        /// </summary>
        /// <param name="parameters">an enumerable of (column, value); the name of each column is used as the name of the parameter</param>
        public void Set(params Tuple<Column, object>[] parameters)
        {
            foreach (Tuple<Column, object> parameter in parameters)
            {
                SetParameter(parameter.Item1.Name, parameter.Item2);
            }
        }


        /// <summary>
        ///     Executes this Query.
        /// </summary>
        /// <returns>the number of affected rows</returns>
        public int ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        /// <summary>
        ///     Executes this query.
        /// </summary>
        /// <returns>a DataTable containing the query's results</returns>
        public DataTable ExecuteQuery()
        {
            DbDataReader reader = _command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }


        /// <summary>
        ///     Prepares a single parameter.
        /// </summary>
        /// <param name="name">the name of the parameter</param>
        /// <param name="type">the data type of the parameter</param>
        private void PrepareParameter(string name, NpgsqlDbType type)
        {
            _command.Parameters.Add(new NpgsqlParameter(name, type));
        }

        /// <summary>
        ///     Sets the value for a single parameter.
        /// </summary>
        /// <param name="name">the name of the parameter</param>
        /// <param name="value">the value of the parameter</param>
        private void SetParameter(string name, object value)
        {
            _command.Parameters[name].Value = value;
        }

        /// <summary>
        ///     Formats the command string.
        /// </summary>
        /// <param name="arguments">the arguments to format with</param>
        private void Format(params object[] arguments)
        {
            _command.CommandText = string.Format(_command.CommandText, arguments);
        }
    }


    /// <summary>
    ///     A column's properties in a table.
    /// </summary>
    internal class Column
    {
        public readonly string Name;
        public readonly NpgsqlDbType Type;


        /// <summary>
        ///     Constructs a new column.
        /// </summary>
        /// <param name="name">the name of the column</param>
        /// <param name="type">the type of the column</param>
        public Column(string name, NpgsqlDbType type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        ///     Constructs a new column.
        /// </summary>
        /// <param name="tuple">a name-type pair</param>
        public Column(Tuple<string, NpgsqlDbType> tuple) : this(tuple.Item1, tuple.Item2)
        {
        }
    }
}
