using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using oServer.DbModels;

namespace oServer
{
    public sealed class MySqlDataAccess
    {
        // MongoClient _client;

        private static readonly MySqlDataAccess instance = new MySqlDataAccess(
            new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .Build());
        private readonly MySqlConnection _connection;

        static MySqlDataAccess()
        {
        }

        private MySqlDataAccess(IConfiguration configuration)
        {
            _connection = new MySqlConnection(configuration.GetValue<string>("DbConnection:ConnectionString"));
        }

        public static MySqlDataAccess Instance
        {
            get
            {
                return instance;
            }
        }

        private void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
        }
        private void CloseConnection()
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }

        public async Task<int> Execute(string query, params object[] parameters)
        {
            try
            {
                OpenConnection();
                using (var command = new MySqlCommand(query, _connection))
                {
                    for (int i = 0; i < parameters.Length; i++)
                        command.Parameters.AddWithValue("@p" + (i + 1), parameters[i]);

                    return await command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public async void Get(string query, Func<DbDataReader, Task> readFromReader, params object[] parameters)
        {
            try
            {
                OpenConnection();
                using (var command = new MySqlCommand(query, _connection))
                {
                    for (int i = 0; i < parameters.Length; i++)
                        command.Parameters.AddWithValue("@p" + (i + 1), parameters[i]);

                    var reader = await command.ExecuteReaderAsync();

                    using (reader)
                    {
                        while (await reader.ReadAsync())
                        {
                            await readFromReader(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}