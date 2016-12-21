using System.Data.SQLite;
using System.IO;


namespace Kragle
{
    /// <summary>
    ///     Database interface.
    /// </summary>
    internal class Database
    {
        private readonly SQLiteConnection _connection;


        /// <summary>
        ///     Establishes a new database connection.
        /// </summary>
        public Database(string databaseFile)
        {
            bool exists = File.Exists(databaseFile);

            _connection = new SQLiteConnection("Data Source=" + databaseFile + ";Version=3;");
            _connection.Open();

            if (!exists)
            {
                InitialiseDatabase(_connection);
            }
        }


        /// <summary>
        ///     Executes the given SQL and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">the SQL to execute</param>
        /// <returns>the number of affected rows</returns>
        public int ExecuteQuery(string sql)
        {
            return new SQLiteCommand(sql, _connection).ExecuteNonQuery();
        }

        /// <summary>
        /// Prepares the given query.
        /// </summary>
        /// <param name="sql">the SQL to prepare</param>
        /// <returns>the prepared SQLiteCommand</returns>
        public SQLiteCommand PrepareQuery(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            command.Prepare();
            return command;
        }

        /// <summary>
        ///     Executes the given SQL and returns the reader of the result.
        /// </summary>
        /// <param name="sql">the SQL to execute</param>
        /// <returns></returns>
        public SQLiteDataReader ReadQuery(string sql)
        {
            return new SQLiteCommand(sql, _connection).ExecuteReader();
        }

        /// <summary>
        ///     Resets the database file, removing all contents.
        /// </summary>
        /// <param name="databaseFile">the location of the database file</param>
        public static void Reset(string databaseFile)
        {
            File.Delete(databaseFile);
        }


        /// <summary>
        ///     Populates the database with the necessary tables and keys.
        /// </summary>
        /// <param name="connection">a valid connection to a database</param>
        private static void InitialiseDatabase(SQLiteConnection connection)
        {
            new SQLiteCommand("CREATE TABLE proto_user (id integer primary key);", connection).ExecuteNonQuery();
        }
    }
}
