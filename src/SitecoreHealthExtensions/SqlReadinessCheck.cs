using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using Sitecore.Pipelines;

namespace SitecoreHealthExtensions
{
    public class SqlReadinessCheck
    {
        private readonly string _connectionString;
        private readonly string _name;
        private readonly Stopwatch _watch;
        private readonly int _commandTimeout;

        public SqlReadinessCheck(string connectionStringName, string connectTimeoutMilliseconds = "1000", string commandTimeoutMilliseconds = "1000")
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionString == null)
            {
                throw new ArgumentException($"ConnectionString '{connectionStringName}' was not found.");
            }

            var builder = new SqlConnectionStringBuilder(connectionString.ConnectionString);

            if (int.TryParse(connectTimeoutMilliseconds, out var connectTimeout))
            {
                if (connectTimeout >= 1000)
                {
                    connectTimeout = connectTimeout / 1000;
                }
                else
                {
                    connectTimeout = 1;
                }

                builder.ConnectTimeout = connectTimeout;
            }

            if (int.TryParse(commandTimeoutMilliseconds, out var commandTimeout))
            {
                if (commandTimeout >= 1000)
                {
                    commandTimeout = commandTimeout / 1000;
                }
                else
                {
                    commandTimeout = 1;
                }

                _commandTimeout = commandTimeout;
            }

            _connectionString = builder.ConnectionString;
            _name = $"{GetType().Name}[{connectionStringName}]";
            _watch = new Stopwatch();
        }

        public void Process(HealthCheckPipelineArgs args)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    _watch.Restart();

                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandTimeout = _commandTimeout;
                        command.CommandText = "SELECT 1";
                        command.ExecuteScalar();
                    }

                    args.AddMessage($"{_name} OK ({_watch.ElapsedMilliseconds}ms)", PipelineMessageType.Information);
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is SqlException || ex is ConfigurationErrorsException)
                {
                    args.AddMessage($"{_name} failed ({_watch.ElapsedMilliseconds}ms)", PipelineMessageType.Error);
                }
                finally
                {
                    _watch.Stop();
                }
            }
        }
    }
}
