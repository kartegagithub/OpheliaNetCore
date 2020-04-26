using Ophelia.Service;
using System.ServiceModel.Description;

namespace Ophelia
{
    public static class ServiceEndPointExtensions
    {
        public static void AddLoggingEndpointBehaviour(this ServiceEndpoint endpoint, ServiceLogHandler logger)
        {
            endpoint.EndpointBehaviors.Add(new LoggingEndpointBehaviour(new LoggingMessageInspector(logger)));
        }
    }
}
