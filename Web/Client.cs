using System;
using System.Threading;
using System.Web;
using Ophelia;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Collections.Generic;

namespace Ophelia.Web
{
    public class Client : IDisposable
    {
        [ThreadStatic]
        protected static Client _Current;

        public static Client Current
        {
            get
            {
                if (Ophelia.Web.View.Mvc.Middlewares.HTTPContextAccessor.Current == null)
                {
                    if (_Current == null)
                    {
                        _Current = (Client)typeof(Client).GetRealTypeInstance(true);
                    }
                    return _Current;
                }
                return (Client)Ophelia.Web.View.Mvc.Middlewares.HTTPContextAccessor.Current.Items["Client"];
            }
        }
        private string sSessionID;
        private string sUserHostAddress = string.Empty;
        public decimal InstanceID { get; set; }
        public string ApplicationName { get; set; }
        public Dictionary<string, object> SharedData { get; set; }
        public Ophelia.Web.View.Mvc.Controllers.Base.Controller Controller { get; set; }
        public HttpContext Context
        {
            get
            {
                return View.Mvc.Middlewares.HTTPContextAccessor.Current;
            }
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
            this.SharedData = null;
        }

        public virtual void Dispose()
        {
            this.Disconnect();
            Debug.WriteLine("Client.Dispose ManagedThreadId: " + Thread.CurrentThread.ManagedThreadId);
        }

        public Client()
        {
            _Current = this;

            var rnd = new Random();
            this.InstanceID = rnd.Next(int.MaxValue);
            rnd = null;
            this.SharedData = new Dictionary<string, object>();
        }
    }
}
