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
        private readonly Stopwatch _watch;
        private readonly string _name;

        public SqlReadinessCheck(string connectionStringName, string timeoutMilliseconds = "1000")
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionString == null)
            {
                throw new ArgumentException($"ConnectionString '{connectionStringName}' was not found.");
            }

            var builder = new SqlConnectionStringBuilder(connectionString.ConnectionString);

            if (int.TryParse(timeoutMilliseconds, out var timeout))
            {
                builder.ConnectTimeout = timeout / 1000;
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
