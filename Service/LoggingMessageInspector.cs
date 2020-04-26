using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Ophelia.Service
{
    public class LoggingMessageInspector : IClientMessageInspector
    {
        public ServiceLogHandler Logger { get; private set; }
        private string URL { get; set; }
        public LoggingMessageInspector(ServiceLogHandler logger)
        {
            this.Logger = logger;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (this.Logger != null)
            {
                using (var buffer = reply.CreateBufferedCopy(int.MaxValue))
                {
                    var document = GetDocument(buffer.CreateMessage());
                    this.Logger.LogResponse(200, this.URL, document.OuterXml);

                    reply = buffer.CreateMessage();
                }
            }
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (this.Logger != null)
            {
                this.URL = channel.RemoteAddress.ToString();
                var headers = request.Headers.ToJson();
                using (var buffer = request.CreateBufferedCopy(int.MaxValue))
                {
                    var document = GetDocument(buffer.CreateMessage());
                    this.Logger.LogRequest(0, this.URL, document.OuterXml, headers);

                    request = buffer.CreateMessage();
                    return null;
                }
            }
            return null;
        }

        private XmlDocument GetDocument(Message request)
        {
            XmlDocument document = new XmlDocument();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // write request to memory stream
                XmlWriter writer = XmlWriter.Create(memoryStream);
                request.WriteMessage(writer);
                writer.Flush();
                memoryStream.Position = 0;

                // load memory stream into a document
                document.Load(memoryStream);
            }

            return document;
        }
    }
}
