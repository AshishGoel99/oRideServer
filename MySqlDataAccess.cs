using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using oServer.DbModels;

namespace oServer
{
    public sealed class MySqlDataAccess
    {
        // MongoClient _client;

        private static readonly MySqlDataAccess instance = new MySqlDataAccess();
        private readonly MySqlConnection _connection;

        static MySqlDataAccess()
        {
        }

        private MySqlDataAccess()
        {
            _connection = new MySqlConnection(Configuration.Config["DbConnection:ConnectionString"]);
        }

        public static MySqlDataAccess Instance
        {
            get
            {
                return instance;
            }
        }

        private async void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.OpenAsync();
        }
        private async void CloseConnection()
        {
            if (_connection.State == ConnectionState.Open)
                _connection.CloseAsync();
        }

        public int Execute(string query, params object[] parameters)
        {
            try
            {
                OpenConnection();
                using (var command = new MySqlCommand(query, _connection))
                {
                    for (int i = 0; i < parameters.Length; i++)
                        command.Parameters.AddWithValue("@p" + (i + 1), parameters[i]);

                    return command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public DataSet Get(string query, params object[] parameters)
        {
            var dataset = new DataSet();
            try
            {
                OpenConnection();
                using (var adap = new MySqlDataAdapter(query, _connection))
                {
                    for (int i = 0; i < parameters.Length; i++)
                        adap.SelectCommand.Parameters.AddWithValue("@p" + (i + 1), parameters[i]);
                    adap.Fill(dataset);
                }

                return dataset;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}