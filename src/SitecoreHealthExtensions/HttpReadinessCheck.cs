using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using Sitecore.Pipelines;

namespace SitecoreHealthExtensions
{
    public class HttpReadinessCheck
    {
        private readonly HttpStatusCode _expectedStatusCode;
        private readonly string _method;
        private readonly string _name;
        private readonly int _timeoutMilliseconds;
        private readonly Stopwatch _watch;
        private readonly Uri _uri;

        public HttpReadinessCheck(string connectionStringName, string timeoutMilliseconds = "1000", string expectedStatusCode = "200", string method = "HEAD", string path = "/")
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionString == null)
            {
                throw new ArgumentException($"ConnectionString '{connectionStringName}' was not found.");
            }

            Enum.TryParse(expectedStatusCode, out _expectedStatusCode);

            int.TryParse(timeoutMilliseconds, out _timeoutMilliseconds);

            var baseUri = new Uri(connectionString.ConnectionString);

            _uri = new Uri(baseUri, path);
            _method = method;
            _name = $"{GetType().Name}[{connectionStringName}]";
            _watch = new Stopwatch();
        }

        public void Process(HealthCheckPipelineArgs args)
        {
            var request = (HttpWebRequest)WebRequest.Create(_uri);

            request.Timeout = _timeoutMilliseconds;
            request.Method = _method;
            request.AllowAutoRedirect = true;

            try
            {
                _watch.Restart();

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var statusCode = response.StatusCode;

                    if (statusCode == _expectedStatusCode)
                    {
                        args.AddMessage($"{_name} OK ({_watch.ElapsedMilliseconds}ms)", PipelineMessageType.Information);
                    }
                    else
                    {
                        args.AddMessage($"{_name} failed ({_watch.ElapsedMilliseconds}ms), status code was {statusCode}", PipelineMessageType.Error);
                    }
                }
            }
            catch
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
