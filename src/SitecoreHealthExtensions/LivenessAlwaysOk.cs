using Sitecore.Pipelines;

namespace SitecoreHealthExtensions
{
    public class LivenessAlwaysOk
    {
        public void Process(HealthCheckPipelineArgs args)
        {
            args.AddMessage("OK", PipelineMessageType.Information);
        }
    }
}
