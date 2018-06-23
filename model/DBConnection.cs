using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ConsoleApp3.model
{
    public class DbConnection
    {
        private DbConnection()
        {
        }

        private const string DatabaseName = "bankd";
        

        private MySqlConnection _connection = null;

        public MySqlConnection Connection
        {
            get { return _connection; }
        }

        private static DbConnection _instance = null;

        public static DbConnection Instance()
        {
            return _instance != null ? _instance : (_instance = new DbConnection());
        }

        public void OpenConnection()
        {
            if (_connection == null)
            {
                var connstring =
                    string.Format(
                        "Server=localhost; database={0}; UID=root; password=; persistsecurityinfo=True;port=3306;SslMode=none",
                        DatabaseName);
                _connection = new MySqlConnection(connstring);
                _connection.Open();
            }
            else if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}