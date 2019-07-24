using System.Collections.Generic;
using System.Linq;
using Sitecore.Pipelines;

namespace SitecoreHealthExtensions
{
    public class HealthCheckPipelineArgs : PipelineArgs
    {
        public List<string> GetAllMessages()
        {
            return GetMessages(PipelineMessageFilter.All).Select(message => message.Text).ToList();
        }

        public bool IsSuccess()
        {
            return GetMessages(PipelineMessageFilter.Errors).Length == 0;
        }
    }
}
