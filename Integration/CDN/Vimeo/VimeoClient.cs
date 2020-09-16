using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Integration.CDN.Vimeo
{
    public delegate void UploadProgressEventHandler(VimeoClient client, FileUploadRequest request, UploadProgressEventArgs e);
    public delegate void TicketReceivedEventHandler(VimeoClient client, FileUploadRequest request, UploadTicket e);
    public delegate void CompletedEventHandler(VimeoClient client, FileUploadRequest request, FileUploadResult result);
    public delegate void FailedEventHandler(VimeoClient client, FileUploadRequest request, FileUploadVerification verification);
    public class VimeoClient
    {
        public string APIURL { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public FileUploadResult Result { get; private set; }

        public event UploadProgressEventHandler UploadProgress;
        public event TicketReceivedEventHandler TicketReceived;
        public event CompletedEventHandler Completed;
        public event FailedEventHandler Failed;

        public VimeoClient()
        {
            this.APIURL = "https://api.vimeo.com";
        }
        public VimeoClient(string clientID, string clientSecret) : this()
        {
            this.ClientID = clientID;
            this.ClientSecret = clientSecret;
        }
        public VimeoClient(string accessToken) : this()
        {
            this.AccessToken = accessToken;
        }
        public string GetAuthURL()
        {
            return this.GetAuthURL(null);
        }
        public string GetAuthURL(List<string> scopes)
        {
            return this.GetAuthURL(scopes, "");
        }
        public string GetAuthURL(List<string> scopes, string redirect)
        {
            List<string> choices = new List<string> { "interact", "private", "public", "create", "edit", "delete", "upload" };
            string authUrl = String.Format("/oauth/authorize?response_type=code&client_id={0}&scope=", this.ClientID);

            if (scopes == null) scopes = choices;
            foreach (var scope in scopes)
            {
                if (!choices.Contains(scope))
                    throw new FormatException(String.Format("Scope must be one of {0} (found scope '{1}')", choices, scope));

                authUrl += String.Format("{0}+", scope);
            }
            authUrl += String.Format("&redirect_uri={0}", redirect);
            return this.APIURL + authUrl;
        }
        public Dictionary<string, object> GetAccessToken(string authCode, string redirect)
        {
            string encoded = Helpers.ToBase64(String.Format("{0}:{1}", this.ClientID, this.ClientSecret));
            var payload = new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"code", authCode},
                {"redirect_uri", redirect}
            };
            var headers = new WebHeaderCollection()
            {
                {"Authorization", String.Format("Basic {0}", encoded)}
            };
            var response = Helpers.HTTPFetch(this.APIURL + "/oauth/access_token", "POST", headers, payload).Read();

            return response.FromJson<Dictionary<string, object>>();
        }
        public string GetClientCredentials(string scopes = null)
        {
            var basicAuth = Helpers.ToBase64(String.Format("{0}:{1}", this.ClientID, this.ClientSecret));
            var payload = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"}
            };
            var headers = new WebHeaderCollection
            {
                {"Authorization", String.Format("Basic {0}", basicAuth)}
            };
            if (scopes != null) payload.Add("scope", scopes);
            var response = Helpers.HTTPFetch(this.APIURL + "/oauth/authorize/client", "POST", headers, payload).Read();
            var json = response.FromJson<Dictionary<string, string>>();
            return json["access_token"];
        }
        public HttpWebResponse RequestResponse(string url, Dictionary<string, string> parameters, string method, bool jsonBody = true)
        {
            var headers = new WebHeaderCollection()
            {
                { "Authorization", String.Format("Bearer {0}", this.AccessToken) }
            };
            method = method.ToUpper();
            if (!url.StartsWith("http"))
                url = this.APIURL + url;
            string body = "";
            string contentType = "application/x-www-form-urlencoded";

            if (parameters != null && parameters.Count > 0)
            {
                if (method == "GET")
                {
                    url += "?" + Helpers.KeyValueToString(parameters);
                }
                else if (method == "POST" || method == "PATCH" || method == "PUT" || method == "DELETE")
                {
                    if (jsonBody)
                    {
                        contentType = "application/json";
                        body = parameters.ToJson();
                    }
                    else
                    {
                        body = Helpers.KeyValueToString(parameters);
                    }
                }
            }

            return Helpers.HTTPFetch(url, method, headers, body, contentType);
        }
        public Dictionary<string, object> Request(string url, Dictionary<string, string> parameters, string method, bool jsonBody = true)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(RequestResponse(url, parameters, method, jsonBody).Read());
        }
        public Dictionary<string, object> GetUser()
        {
            return this.Request("/me", null, "GET");
        }
        public Dictionary<string, object> UpdateVideo(string url, dynamic data, string method = "PATCH")
        {
            return this.UpdateVideo(url, Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(Newtonsoft.Json.JsonConvert.SerializeObject(data)), method);
        }
        public Dictionary<string, object> UpdateVideo(string url, Dictionary<string, string> data, string method = "PATCH")
        {
            return this.Request(url, data, method, false);
        }
        public Dictionary<string, object> GetVideos()
        {
            return this.Request("/me/videos", null, "GET");
        }
        public UploadTicket GetUploadTicket(FileUploadRequest request, string redirect_uri = "", string type = "streaming")
        {
            try
            {
                if (request.Ticket == null)
                {
                    var ticket = new UploadTicket();
                    var result = this.Request("/me/videos?type=" + type + "&redirect_url=" + redirect_uri, null, "POST");
                    ticket.URI = Convert.ToString(result["upload_link"]);
                    ticket.CompleteURI = this.APIURL + Convert.ToString(result["complete_uri"]);
                    ticket.TicketID = Convert.ToString(result["ticket_id"]);
                    ticket.UploadLinkSecure = Convert.ToString(result["upload_link_secure"]);
                    this.OnTicketReceived(request, ticket);
                    return ticket;
                }
                else
                    return request.Ticket;
            }
            catch (Exception ex)
            {
                this.OnFailed(request, ex);
                return null;
            }
        }

        public Dictionary<string, object> Request(string uri, string method = "GET")
        {
            try
            {
                return this.Request(uri, null, method);
            }
            catch (Exception ex)
            {
                this.OnFailed(null, ex);
                return null;
            }
        }
        public FileUploadVerification VerifyUpload(long localFileSize, string uri, string ticket_id, string method = "PUT")
        {
            var result = new FileUploadVerification();
            var headers = new WebHeaderCollection();
            headers.Add("Content-Range", "bytes */*");
            var response = Helpers.HTTPFetch(uri, method, headers, "");
            if (Convert.ToInt32(response.StatusCode) == 308)
                result.Status = true;
            if (!string.IsNullOrEmpty(response.Headers["Range"]))
                result.FileSize = Convert.ToInt64(response.Headers["Range"].Split('-')[1]);
            if (result.Status && result.FileSize == localFileSize)
                result.Status = true;
            var strResponse = response.Read();

            this.OnVerifyUpload(strResponse, result);

            return result;
        }
        protected virtual void OnVerifyUpload(string strResponse, FileUploadVerification result)
        {

        }
        public string Upload(long fromByte, long ToByte, long TotalByte, string uri, byte[] data, string method = "PUT")
        {
            var headers = new WebHeaderCollection();
            headers.Add("Content-Range", "bytes " + fromByte + "-" + ToByte + "/" + TotalByte);
            var response = PostForm(uri, "", "video/mp4", data, headers, method);
            return response.Read();
        }
        public virtual void ProgressiveUpload(FileUploadRequest request)
        {
            this.Result = new FileUploadResult() { };
            try
            {
                this.Result.InProcess = true;

                if (System.IO.File.Exists(request.Path))
                {
                    var ticket = this.GetUploadTicket(request);
                    if (ticket != null)
                    {
                        request.Ticket = ticket;
                        using (var stream = new FileStream(request.Path, FileMode.Open))
                        {
                            var total = stream.Length;
                            FileUploadVerification verificationResult = null;
                            if (request.FromRange == 0)
                            {
                                verificationResult = this.VerifyUpload(total, request.Ticket.UploadLinkSecure, request.Ticket.TicketID);
                                request.FromRange = verificationResult.FileSize;
                            }

                            if (request.FromRange > 0 && stream.CanSeek && stream.CanRead)
                            {
                                stream.Seek(request.FromRange, SeekOrigin.Begin);
                            }
                            var uploaded = request.FromRange;
                            var remaining = total - uploaded;
                            var data = new byte[request.ChunkSize];
                            while (stream.Read(data, 0, data.Length) > 0)
                            {
                                this.Upload(uploaded, uploaded + data.Length, total, request.Ticket.UploadLinkSecure, data);
                                uploaded += data.Length;
                                remaining -= data.Length;

                                this.Result.UploadedFileLength = uploaded;
                                this.OnUploadProgress(request, new UploadProgressEventArgs() { Total = total, Uploaded = uploaded, Remaining = remaining });
                            }
                            verificationResult = this.VerifyUpload(uploaded, request.Ticket.UploadLinkSecure, request.Ticket.TicketID);
                            if (!verificationResult.Status)
                            {
                                this.Result.Failed = true;
                                this.Result.Description = "Something went wrong while file upload. Uploaded file does not match with sent file";
                                this.Result.UploadedFileLength = verificationResult.FileSize;
                                this.OnFailed(request, verificationResult);
                            }
                            else
                            {
                                this.Result.UploadedFile = this.CompleteUpload(request.Ticket.CompleteURI);
                                this.OnCompleted(request, this.Result);
                            }
                        }
                    }
                    else
                    {
                        this.Result.Failed = true;
                        this.Result.Description = "Could not get ticket. Access token is not valid to do this operation";
                    }
                }
                else
                {
                    this.Result.Failed = true;
                    this.Result.Description = "File does not exist";
                }
            }
            catch (Exception ex)
            {
                this.Result.Failed = true;
                this.Result.Description = ex.Message + " " + ex.StackTrace;
            }
            finally
            {
                this.Result.InProcess = false;
            }
        }
        protected virtual void OnUploadProgress(FileUploadRequest request, UploadProgressEventArgs e)
        {
            UploadProgress?.Invoke(this, request, e);
        }
        protected virtual void OnTicketReceived(FileUploadRequest request, UploadTicket ticket)
        {
            TicketReceived?.Invoke(this, request, ticket);
        }
        protected virtual void OnCompleted(FileUploadRequest request, FileUploadResult result)
        {
            Completed?.Invoke(this, request, result);
        }
        protected virtual void OnFailed(FileUploadRequest request, string description)
        {

        }
        protected virtual void OnFailed(FileUploadRequest request, Exception ex)
        {

        }
        protected virtual void OnFailed(FileUploadRequest request, FileUploadVerification verification)
        {
            Failed?.Invoke(this, request, verification);
        }
        public string CompleteUpload(string completeURL)
        {
            var response = this.RequestResponse(completeURL, null, "DELETE");
            string location = Convert.ToString(response.Headers["Location"]);
            this.OnCompleteUpload(response, string.IsNullOrEmpty(location) ? "" : location);
            response.Close();
            return location;
        }
        protected virtual void OnCompleteUpload(HttpWebResponse response, string location)
        {

        }
        private readonly Encoding encoding = Encoding.UTF8;
        private HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData, WebHeaderCollection headers, string method = "POST")
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            request.Headers.Add(headers);

            request.Method = method;
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.ContentLength = formData.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponseWithoutException();
        }
    }
}
