using System;
using System.Threading;
using System.Web;
using Ophelia;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Ophelia.Web
{
    public class Client : IDisposable
    {
        protected static AsyncLocal<Client> _Current;
        public static Client Current
        {
            get
            {
                return _Current.Value;
            }
        }
        private string sSessionID;
        private string sUserHostAddress = string.Empty;
        public decimal InstanceID { get; set; }
        public string ApplicationName { get; set; }
        public Ophelia.Web.View.Mvc.Controllers.Base.Controller Controller { get; set; }
        public HttpContext Context
        {
            get;
            set;
        }

        public ISession Session
        {
            get { return this.Context.Session; }
        }
        public HttpResponse Response
        {
            get { return this.Context.Response; }
        }
        public HttpRequest Request
        {
            get { return this.Context.Request; }
        }
        public string ComputerName
        {
            get { return System.Net.Dns.GetHostName(); }
        }
        public string UserHostAddress
        {
            get
            {
                if (string.IsNullOrEmpty(this.sUserHostAddress)) this.sUserHostAddress = this.Context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                return this.sUserHostAddress;
            }
        }
        public string UserAgent
        {
            get
            {
                return this.Request.Headers["User-Agent"];
            }
        }
        public string SessionID
        {
            get
            {
                if (string.IsNullOrEmpty(this.sSessionID)) this.sSessionID = this.Session.Id;
                return this.sSessionID;
            }
        }
        public virtual int CurrentLanguageID { get; set; }
        public virtual string TranslateText(string Text)
        {
            return Text;
        }
        public virtual string GetImagePath(string path, bool forListing = true)
        {
            return path;
        }
        public virtual void Disconnect()
        {
            _Current = null;
        }

        public virtual void Dispose()
        {
            this.Disconnect();
        }

        public Client()
        {
            _Current.Value = this;
            var rnd = new Random();
            this.InstanceID = rnd.Next(int.MaxValue);
            rnd = null;
        }
    }
}
