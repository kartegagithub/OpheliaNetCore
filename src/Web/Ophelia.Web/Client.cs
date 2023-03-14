using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Ophelia.Web
{
    public class Client : Ophelia.Application.ApplicationClient, IDisposable
    {
        public new static Client Current
        {
            get
            {
                if (View.Mvc.Middlewares.HTTPContextAccessor.Current != null)
                    _Current = (Client)View.Mvc.Middlewares.HTTPContextAccessor.Current.Items["Client"];

                if (_Current == null)
                {
                    _Current = (Client)typeof(Client).GetRealTypeInstance(true);
                    if (View.Mvc.Middlewares.HTTPContextAccessor.Current != null)
                        View.Mvc.Middlewares.HTTPContextAccessor.Current.Items["Client"] = _Current;
                }

                return (Client)_Current;
            }
        }
        public Web.View.Mvc.Controllers.Base.Controller Controller { get; set; }
        public HttpContext Context => View.Mvc.Middlewares.HTTPContextAccessor.Current;
        public ISession Session
        {
            get
            {
                try
                {
                    return this.Context != null ? this.Context.Session : null;
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
                    return this.Context != null ? this.Context.Response : null;
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
                    return this.Context != null ? this.Context.Request : null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override string UserHostAddress
        {
            get
            {
                if (string.IsNullOrEmpty(base.UserHostAddress)) base.UserHostAddress = this.Context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                return base.UserHostAddress;
            }
        }
        public override string UserAgent
        {
            get
            {
                if (this.Request != null && this.Request.Headers.ContainsKey("User-Agent") && this.Request.Headers.TryGetValue("User-Agent", out Microsoft.Extensions.Primitives.StringValues value) && !string.IsNullOrEmpty(value))
                    return value;
                return "";
            }
        }

        public override string SessionID
        {
            get
            {
                if (string.IsNullOrEmpty(base.SessionID))
                {
                    if (this.Session != null)
                        base.SessionID = this.Session.Id;
                    else
                        base.SessionID = Guid.NewGuid().ToString();
                }
                return base.SessionID;
            }
        }
        public override int CurrentLanguageID
        {
            get
            {
                if (this.Context != null)
                {
                    if (base.CurrentLanguageID == 0)
                    {
                        if (this.Session != null)
                        {
                            if (this.Session.GetString("CurrentLanguageID") != null)
                                base.CurrentLanguageID = this.Session.GetInt32("CurrentLanguageID").GetValueOrDefault(0);
                            else if (base.CurrentLanguageID > 0)
                                this.Session.SetInt32("CurrentLanguageID", base.CurrentLanguageID);
                        }
                        if (base.CurrentLanguageID == 0)
                            base.CurrentLanguageID = this.GetCurrentLanguageCookie();
                    }
                    else
                    {
                        if (this.Session != null && this.Session.GetString("CurrentLanguageID") == null)
                            this.Session.SetInt32("CurrentLanguageID", base.CurrentLanguageID);
                    }
                }
                return base.CurrentLanguageID;
            }
            set
            {
                base.CurrentLanguageID = value;
                if (this.Context != null)
                {
                    this.SetCurrentLanguageCookie();
                    this.Session?.SetInt32("CurrentLanguageID", value);
                }
            }
        }
        protected virtual void SetCurrentLanguageCookie(string cookieName = "Language", CookieOptions options = null)
        {
            options ??= new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(365),
                Path = "/",
                HttpOnly = true,
                Secure = this.Request.IsHttps
            };
            Application.Client.CookieManager.Set(cookieName, $"{this.CurrentLanguageID}", options);
        }

        protected virtual int GetCurrentLanguageCookie(string cookieName = "Language")
        {
            var languageCookies = Web.Application.Client.CookieManager.Get(cookieName);
            int ID = 1;
            if (!string.IsNullOrEmpty(languageCookies))
            {
                if (int.TryParse(languageCookies, out ID))
                    return ID;
            }
            return ID;
        }

        public Client() : base()
        {

        }
    }
}