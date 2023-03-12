using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Ophelia.Web
{
    public class Client : IDisposable
    {
        [ThreadStatic]
        protected static Client _Current;

        protected int nCurrentLanguageID = 0;

        public static Client Current
        {
            get
            {
                if (Ophelia.Web.View.Mvc.Middlewares.HTTPContextAccessor.Current != null)
                    _Current = (Client)Ophelia.Web.View.Mvc.Middlewares.HTTPContextAccessor.Current.Items["Client"];

                if (_Current == null)
                {
                    _Current = (Client)typeof(Client).GetRealTypeInstance(true);
                    if (Ophelia.Web.View.Mvc.Middlewares.HTTPContextAccessor.Current != null)
                        Ophelia.Web.View.Mvc.Middlewares.HTTPContextAccessor.Current.Items["Client"] = _Current;
                }

                return _Current;
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
            get
            {
                try
                {
                    if (this.Context != null)
                        return this.Context.Session;
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public HttpResponse Response
        {
            get
            {
                try
                {
                    if (this.Context != null)
                        return this.Context.Response;
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public HttpRequest Request
        {
            get
            {
                try
                {
                    if (this.Context != null)
                        return this.Context.Request;
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
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
                if (string.IsNullOrEmpty(this.sSessionID))
                {
                    if (this.Session != null)
                        this.sSessionID = this.Session.Id;
                    else
                        this.sSessionID = Guid.NewGuid().ToString();
                }
                return this.sSessionID;
            }
        }
        public virtual int CurrentLanguageID
        {
            get
            {
                if (this.Context != null)
                {
                    if (this.nCurrentLanguageID == 0)
                    {
                        if (this.Session != null)
                        {
                            if (this.Session.GetString("CurrentLanguageID") != null)
                                this.nCurrentLanguageID = this.Session.GetInt32("CurrentLanguageID").GetValueOrDefault(0);
                            else if (this.nCurrentLanguageID > 0)
                                this.Session.SetInt32("CurrentLanguageID", this.nCurrentLanguageID);
                        }
                        if (this.nCurrentLanguageID == 0)
                            this.nCurrentLanguageID = this.GetCurrentLanguageCookie();
                    }
                    else
                    {
                        if (this.Session != null && this.Session.GetString("CurrentLanguageID") == null)
                            this.Session.SetInt32("CurrentLanguageID", this.nCurrentLanguageID);
                    }
                }
                return this.nCurrentLanguageID;
            }
            set
            {
                this.nCurrentLanguageID = value;
                if (this.Context != null)
                {
                    this.SetCurrentLanguageCookie();
                    if (this.Session != null)
                        this.Session.SetInt32("CurrentLanguageID", value);
                }
            }
        }
        protected virtual void SetCurrentLanguageCookie(string cookieName = "Language", CookieOptions options = null)
        {
            if (options == null)
            {
                options = new CookieOptions()
                {
                    Expires = DateTime.Now.AddDays(365),
                    Path = "/",
                    HttpOnly = true,
                    Secure = this.Request.IsHttps
                };
            }
            Ophelia.Web.Application.Client.CookieManager.Set(cookieName, this.CurrentLanguageID.ToString(), options);
        }

        protected virtual int GetCurrentLanguageCookie(string cookieName = "Language")
        {
            var languageCookies = Ophelia.Web.Application.Client.CookieManager.Get(cookieName);
            int ID = 1;
            if (!string.IsNullOrEmpty(languageCookies))
            {
                if (int.TryParse(languageCookies, out ID))
                    return ID;
            }
            return 0;
        }
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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