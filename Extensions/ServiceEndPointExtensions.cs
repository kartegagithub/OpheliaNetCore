using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;

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
