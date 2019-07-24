using System;
using System.Diagnostics;
using System.Net.Http;
using Sitecore.Pipelines;
using Sitecore.Pipelines.PreprocessRequest;

namespace SitecoreHealthExtensions
{
    public class SwitchHealthRequest : PreprocessRequestProcessor
    {
        private const string LivenessHealthCheckPipelineName = "livenessHealthCheck";
        private const string ReadinessHealthCheckPipelineName = "readinessHealthCheck";
        private readonly string _livenessPath;
        private readonly string _readinessPath;

        public SwitchHealthRequest(string livenessPath, string readinessPath)
        {
            _livenessPath = livenessPath;
            _readinessPath = readinessPath;
        }

        public override void Process(PreprocessRequestArgs args)
        {
            var context = args.HttpContext;

            if (context.Request.HttpMethod != HttpMethod.Get.Method && context.Request.HttpMethod != HttpMethod.Head.Method)
            {
                return;
            }

            if (!(context.Request.Path.Equals(_livenessPath, StringComparison.OrdinalIgnoreCase) ||
                  context.Request.Path.Equals(_readinessPath, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            args.AbortPipeline();

            var pipelineArgs = new HealthCheckPipelineArgs();

            if (context.Request.Path.Equals(_livenessPath, StringComparison.OrdinalIgnoreCase))
            {
                CorePipeline.Run(LivenessHealthCheckPipelineName, pipelineArgs, true);
            }
            else if (context.Request.Path.Equals(_readinessPath, StringComparison.OrdinalIgnoreCase))
            {
                CorePipeline.Run(ReadinessHealthCheckPipelineName, pipelineArgs, true);
            }

            context.Response.ContentType = "text/plain";

            foreach (var message in pipelineArgs.GetAllMessages())
            {
                context.Response.Write(message + Environment.NewLine);
            }

            if (pipelineArgs.IsSuccess())
            {
                context.Response.StatusCode = 200;
            }
            else
            {
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = 503;
            }

            context.ApplicationInstance.CompleteRequest();
        }
    }
}
