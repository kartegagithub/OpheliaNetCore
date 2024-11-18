using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.Net.Http
{
    public class RequestFactory : IDisposable
    {
        public HttpClient Client { get; private set; }
        public HttpClientHandler ClientHandler { get; private set; }
        public string UserAgent { get; set; }
        public string AuthorizationScheme { get; set; }
        public string AuthorizationValue { get; set; }
        public List<string> Accept { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public HttpRequestMessage RequestMessage { get; private set; }
        public HttpResponseMessage ResponseMessage { get; private set; }
        public Func<HttpClientHandler, DelegatingHandler> LogHandler { get; set; }
        public Action<HttpResponseMessage> OnResponse { get; set; }
        private RequestFactory CreateClientIfNotSet()
        {
            if (this.Client == null)
                this.CreateClient();
            return this;
        }
        public RequestFactory CreateClient()
        {
            return this.CreateClient(null);
        }
        public RequestFactory CreateClient(HttpClientHandler handler)
        {
            this.Reset();
            if (handler == null)
            {
                handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            this.ClientHandler = handler;
            if (this.LogHandler != null)
                this.Client = new HttpClient(this.LogHandler(this.ClientHandler));
            else
                this.Client = new HttpClient(this.ClientHandler);
            this.Client.DefaultRequestHeaders.Add("User-Agent", this.UserAgent);
            return this;
        }
        public RequestFactory CreateClient(IWebProxy proxy, HttpClientHandler handler)
        {
            this.CreateClient(handler);
            this.ClientHandler.Proxy = proxy;
            this.ClientHandler.UseProxy = proxy != null;
            return this;
        }
        public RequestFactory CreateClient(string baseUrl, IWebProxy proxy = null)
        {
            this.CreateClient(proxy, null);
            return this;
        }
        public RequestFactory SetUserAgent(string userAgent)
        {
            this.UserAgent = userAgent;
            return this;
        }
        public RequestFactory SetLogHandler(Func<HttpClientHandler, DelegatingHandler> logHandler)
        {
            this.LogHandler = logHandler;
            return this;
        }
        public RequestFactory SetOnResponse(Action<HttpResponseMessage> handler)
        {
            this.OnResponse = handler;
            return this;
        }
        public RequestFactory AddHeaders(Dictionary<string, string> headers)
        {
            if (headers == null)
                return this;
            this.Headers.AddRange(headers);
            return this;
        }
        public RequestFactory AddHeaders(WebHeaderCollection headers)
        {
            if (headers == null)
                return this;

            foreach (string item in headers.Keys)
            {
                this.AddHeader(item, headers[item]);
            }
            return this;
        }
        public RequestFactory AddHeader(string key, string value)
        {
            this.Headers.Add(key, value);
            return this;
        }
        public RequestFactory AddAccept(string type)
        {
            this.Accept.Add(type);
            return this;
        }
        public RequestFactory SetAuthorization(string scheme, string value)
        {
            this.AuthorizationScheme = scheme;
            this.AuthorizationValue = value;
            return this;
        }
        public RequestFactory SetTimeout(int ms)
        {
            this.CreateClientIfNotSet();
            this.Client.Timeout = TimeSpan.FromMilliseconds(ms);
            return this;
        }
        public RequestFactory SetCredentials(ICredentials credential)
        {
            if (credential == null)
                return this;

            this.CreateClientIfNotSet();
            this.ClientHandler.Credentials = credential;
            return this;
        }
        public RequestFactory SetPreAuthenticate(bool preAuthenticate)
        {
            this.CreateClientIfNotSet();
            this.ClientHandler.PreAuthenticate = preAuthenticate;
            return this;
        }
        public async Task<HttpResponseMessage> SendAsync()
        {
            this.CreateClientIfNotSet();

            var authHeader = this.Headers.Where(op => op.Key.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase)).Select(op => op.Value).FirstOrDefault();
            var acceptHeader = this.Headers.Where(op => op.Key.Equals("Accept", StringComparison.InvariantCultureIgnoreCase)).Select(op => op.Value).FirstOrDefault();
            if (!this.Accept.Any())
                this.Accept.Add(acceptHeader);

            if (!string.IsNullOrEmpty(this.AuthorizationValue) && !string.IsNullOrEmpty(this.AuthorizationScheme))
                this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(this.AuthorizationScheme, this.AuthorizationValue);
            else if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.IndexOf(' ') > -1)
                    this.AuthorizationScheme = authHeader.Left(authHeader.IndexOf(' '));
                if (!string.IsNullOrEmpty(this.AuthorizationScheme))
                    this.AuthorizationValue = authHeader.Replace(this.AuthorizationScheme, "").Trim();
                else
                    this.AuthorizationValue = authHeader;
                if (!string.IsNullOrEmpty(this.AuthorizationScheme) && !string.IsNullOrEmpty(authHeader))
                    this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(this.AuthorizationScheme, this.AuthorizationValue);
            }
            if (this.Headers.Any())
            {
                foreach (var item in this.Headers)
                {
                    if (!item.Key.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase))
                        this.RequestMessage.Content.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }
            if (!this.Accept.Any())
                this.Accept.Add("*/*;q=0.8");

            if (this.Accept.Any())
                this.Accept.ForEach(item => this.Client.DefaultRequestHeaders.Accept.TryParseAdd(item));

            this.ResponseMessage = await this.Client.SendAsync(this.RequestMessage);
            if (this.OnResponse != null)
                this.OnResponse(this.ResponseMessage);

            return this.ResponseMessage;
        }

        public async Task<string> GetStringResponseAsync()
        {
            var result = await this.SendAsync();
            return await result.Content.ReadAsStringAsync();
        }
        public string GetStringResponse()
        {
            return this.GetStringResponseAsync().Result;
        }
        public async Task<T> GetJsonResponseAsync<T>()
        {
            var result = await this.SendAsync();
            return await result.Content.ReadFromJsonAsync<T>();
        }
        public T GetJsonResponse<T>()
        {
            return this.GetJsonResponseAsync<T>().Result;
        }
        public RequestFactory CreateStringContent(string payload, string mediaType = "application/x-www-form-urlencoded")
        {
            this.RequestMessage.Content = new StringContent(payload, Encoding.UTF8, mediaType);
            return this;
        }
        public RequestFactory CreateByteContent(byte[] payload, string mediaType = "application/x-www-form-urlencoded")
        {
            this.RequestMessage.Content = new ByteArrayContent(payload);
            this.RequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            return this;
        }
        public RequestFactory CreateJsonContent(object entity, string mediaType = "application/json", JsonSerializerOptions options = null)
        {
            this.RequestMessage.Content = JsonContent.Create(entity, entity.GetType(), new MediaTypeHeaderValue(mediaType), options);
            return this;
        }
        public RequestFactory CreateJsonContent<T>(T entity, string mediaType = "application/json", JsonSerializerOptions options = null)
        {
            this.RequestMessage.Content = JsonContent.Create(entity, new MediaTypeHeaderValue(mediaType), options);
            return this;
        }
        public void Reset()
        {
            if (this.ClientHandler != null)
                this.ClientHandler.Dispose();
            if (this.Client != null)
                this.Client.Dispose();
            if (this.RequestMessage != null)
                this.RequestMessage.Dispose();

            this.Client = null;
            this.ClientHandler = null;
            this.RequestMessage = null;
        }
        public RequestFactory CreateRequest(string url, string method)
        {
            this.RequestMessage = new HttpRequestMessage(HttpClientExtensions.GetHttpMethod(method), url);
            return this;
        }
        public void Dispose()
        {
            this.Reset();
            this.Accept = null;
            this.Headers = null;
            this.UserAgent = "";
        }

        public RequestFactory()
        {
            this.Accept = new List<string>();
            this.Headers = new Dictionary<string, string>();
            this.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ru; rv:1.9.2.3) Gecko/20100401 Firefox/4.0 (.NET CLR 3.5.30729) Ophelia Request Factory";
        }
    }
}
