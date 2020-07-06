using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ophelia.Service
{
    public abstract class ServiceLogHandler : DelegatingHandler
    {
        public ServiceLogHandler() : base()
        {
        }

        public ServiceLogHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {

        }
        public static ServiceLogHandler CreateInstance(HttpMessageHandler innerHandler)
        {
            return ((ServiceLogHandler)typeof(ServiceLogHandler).GetRealTypeInstance(false, innerHandler));
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;

            var requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);
            try
            {
                var requestMessage = new byte[0];
                var headers = "";
                if (request.Content != null)
                {
                    requestMessage = request.Content.ReadAsByteArrayAsync().Result;
                }
                if (request.Headers != null)
                {
                    headers = request.Headers.ToJson();
                }
                this.LogRequest(0, requestInfo, Encoding.UTF8.GetString(requestMessage), headers);
            }
            catch (System.Exception ex)
            {
                new ServiceObjectResult<bool>().Fail(ex);
            }

            response = base.SendAsync(request, cancellationToken).Result;

            try
            {
                var httpStatus = (int)response.StatusCode;

                byte[] responseMessage;

                if (response.Content != null)
                {
                    responseMessage = response.Content.ReadAsByteArrayAsync().Result;
                }
                else
                {
                    responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);
                }

                this.LogResponse(httpStatus, requestInfo, responseMessage);
            }
            catch (System.Exception ex)
            {
                new ServiceObjectResult<bool>().Fail(ex);
            }
            
            return Task<HttpResponseMessage>.Factory.StartNew(() => response);
        }

        public virtual void LogRequest(int status, string requestInfo, string message, string headers)
        {

        }
        public virtual void LogResponse(int status, string requestInfo, byte[] message)
        {

        }
        public virtual void LogResponse(int status, string requestInfo, string message)
        {

        }
    }
}
