using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ophelia.Service;
using Ophelia.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Web.View.Mvc.Controls.Binders.EntityBinder
{
    public class EntityBinder<T> : Panel, IDisposable where T : class
    {
        private readonly ViewContext viewContext;
        private Boolean isDisposed;
        public HttpRequest Request { get { return this.Client.Request; } }
        public T Entity { get; private set; }
        public long EntityID { get; private set; }
        public Controllers.Base.Controller Controller { get { return this.Client.Controller; } }
        public Client Client { get { return Client.Current; } }
        public Panel Content { get; set; }
        public Form Form { get; set; }
        public string Title { get; set; }
        public TabControl<T> TabControl { get; protected set; }
        public Panel PageTitles { get; private set; }
        public Panel HeadingElements { get; private set; }
        public List Breadcrumb { get; private set; }
        public List ActionButtons { get; private set; }
        public Configuration Configuration { get; private set; }
        public List<ServiceResultMessage> Messages { get; set; }
        public Dictionary<string, List<Expression<Func<T, object>>>> ModeFields { get; set; }
        public Dictionary<string, List<string>> ModeTextFields { get; set; }
        public IUrlHelper Url
        {
            get
            {
                return this.Client.Controller.Url;
            }
        }
        public virtual bool IsAjaxEntityBinderRequest
        {
            get
            {
                return this.Request.GetValue("ajaxentitybinder") == "1";
            }
        }
        public bool Has_i18n
        {
            get
            {
                return this.Entity.GetType().GetProperty("I18n") != null || this.Entity.GetType().GetProperty(this.Entity.GetType().Name + "_i18n") != null;
            }
        }
        public virtual string[] DefaultEntityProperties
        {
            get
            {
                return null;
            }
        }
        public virtual int CurrentLanguageID
        {
            get
            {
                return this.Client.CurrentLanguageID;
            }
        }

        public EntityBinder(T entity, string title)
        {
            this.Entity = entity;
            this.ModeFields = new Dictionary<string, List<Expression<Func<T, object>>>>();
            this.ModeTextFields = new Dictionary<string, List<string>>();
            this.EntityID = Convert.ToInt64(entity.GetPropertyValue("ID"));
            this.Configuration = new Configuration();
            this.CreateTabControl();
            this.Content = new Panel();
            this.Form = new Form();
            this.Breadcrumb = new List();
            this.Breadcrumb.CssClass = "breadcrumb";
            this.ActionButtons = new List();
            this.ActionButtons.CssClass = "breadcrumb-elements";

            this.PageTitles = new Panel();
            this.HeadingElements = new Panel();

            this.Controls.Add(this.Breadcrumb);
            this.Controls.Add(this.ActionButtons);
            this.Controls.Add(this.Content);
            this.Content.Controls.Add(this.Form);
            this.Form.Controls.Add(this.TabControl);

            this.Form.AddAttribute("method", "post");
            this.Form.AddAttribute("enctype", "multipart/form-data");
            this.Form.CssClass = "form-horizontal";

            this.Content.CssClass = "binder-body panel-body";
            this.CssClass = "entity-binder panel panel-flat";

            this.ID = title;
            this.Title = this.Client.TranslateText(title);
            this.OnBeforeConfigure();
            this.Configure();
            this.OnAfterConfigure();
        }

        public EntityBinder(ViewContext viewContext, T entity, string title) : this(entity, title)
        {
            if (viewContext == null)
                throw new ArgumentNullException("viewContext");

            this.viewContext = viewContext;
            if (viewContext.ViewBag.Messages != null)
            {
                this.Messages = viewContext.ViewBag.Messages;
            }
            this.Output = this.viewContext.Writer;
            this.onViewContextSet();
        }
        public EntityBinder<T> DiscardDocumentation(Expression<Func<T, object>> expression)
        {
            return this.DiscardDocumentation(expression.Body.ParsePath());
        }
        public EntityBinder<T> DiscardDocumentation(string Key)
        {
            if (!this.Configuration.Help.DiscardedDocumentation.Contains(Key))
                this.Configuration.Help.DiscardedDocumentation.Add(Key);

            return this;
        }
        public WebControl AddHeadingElementButton(string url, string text, bool openInNewWindow, string cssClass = "btn btn-link btn-float has-text")
        {
            var control = new Link() { URL = url, Text = text, NewWindow = openInNewWindow, CssClass = CssClass };
            this.HeadingElements.Controls.Add(control);
            return control;
        }
        public WebControl AddHeadingElementHtml(string html)
        {
            var control = new Literal() { Text = html };
            this.HeadingElements.Controls.Add(control);
            return control;
        }
        public WebControl AddPageTitleLink(string url, string text, bool openInNewWindow, string cssClass = "")
        {
            var control = new Link() { URL = url, Text = text, NewWindow = openInNewWindow, CssClass = CssClass };
            this.PageTitles.Controls.Add(control);
            return control;
        }
        public WebControl AddPageTitleH4(string text)
        {
            var control = new Literal() { Text = "<h4>" + text + "</h4>" };
            this.PageTitles.Controls.Add(control);
            return control;
        }
        public WebControl AddPageTitleHtml(string html)
        {
            var control = new Literal() { Text = html };
            this.PageTitles.Controls.Add(control);
            return control;
        }
        public EntityBinder<T> AddSearchHelp(Expression<Func<T, object>> expression, string URL, string Callback = "")
        {
            return this.AddSearchHelp(expression.Body.ParsePath(), URL, Callback);
        }
        public EntityBinder<T> AddSearchHelp(string Key, string URL, string Callback = "")
        {
            if (!this.Configuration.Help.SearchHelps.Where(op => op.Path == Key).Any())
                this.Configuration.Help.SearchHelps.Add(new SearchHelp() { Path = Key, URL = URL, Callback = Callback });

            return this;
        }
        public EntityBinder<T> AddDocumentation(Expression<Func<T, object>> expression, string HelpTip)
        {
            return this.AddDocumentation(expression.Body.ParsePath(), HelpTip);
        }
        public EntityBinder<T> AddDocumentation(string Key, string HelpTip)
        {
            if (!this.Configuration.Help.Documentation.ContainsKey(Key))
                this.Configuration.Help.Documentation.Add(Key, HelpTip);
            else
                this.Configuration.Help.Documentation[Key] = HelpTip;

            return this;
        }
        public EntityBinder<T> RemoveDocumentation(Expression<Func<T, object>> expression)
        {
            return this.RemoveDocumentation(expression.Body.ParsePath());
        }
        public EntityBinder<T> RemoveDocumentation(string Key)
        {
            if (this.Configuration.Help.Documentation.ContainsKey(Key))
                this.Configuration.Help.Documentation.Remove(Key);

            return this;
        }
        public EntityBinder<T> AddFieldToMode(string Mode, string key)
        {
            if (!this.ModeTextFields.ContainsKey(Mode))
                this.ModeTextFields.Add(Mode, new List<string>());

            if (key != null)
                this.ModeTextFields[Mode].Add(key);
            return this;
        }
        public EntityBinder<T> AddFieldsToMode(string Mode, params string[] keys)
        {
            if (keys != null)
            {
                foreach (var item in keys)
                {
                    this.AddFieldToMode(Mode, item);
                }
            }
            return this;
        }
        public EntityBinder<T> AddFieldToMode(string Mode, Expression<Func<T, object>> expression)
        {
            if (!this.ModeFields.ContainsKey(Mode))
                this.ModeFields.Add(Mode, new List<Expression<Func<T, object>>>());

            if (expression != null)
                this.ModeFields[Mode].Add(expression);
            return this;
        }
        public EntityBinder<T> AddFieldsToMode(string Mode, params Expression<Func<T, object>>[] expressions)
        {
            if (expressions != null)
            {
                foreach (var item in expressions)
                {
                    this.AddFieldToMode(Mode, item);
                }
            }
            return this;
        }
        protected virtual void Configure()
        {

        }
        protected virtual void OnBeforeConfigure()
        {

        }
        protected virtual void OnAfterConfigure()
        {

        }
        protected virtual void onViewContextSet()
        {

        }

        public HtmlString Render()
        {
            return new HtmlString(base.Draw());
        }
        protected virtual void CreateTabControl()
        {
            this.TabControl = new TabControl<T>(this);
        }

        public virtual void RenderHeader()
        {
            this.RenderBreadcrumb();
            this.Output.Write("<div class=\"content\">");
            this.Output.Write("<form class=\"form-horizontal\">");
            this.TabControl.RenderHeader();
        }

        public virtual void RenderFooter()
        {
            this.TabControl.RenderFooter();

            this.Output.Write("</form>");
            this.Output.Write("</div>"); /* content */
        }
        public virtual void RenderBreadcrumb()
        {
            this.Output.Write("<div class=\"page-header\">");
            this.Output.Write("<div class=\"breadcrumb-line breadcrumb-line-component\">");
            this.Output.Write(this.Breadcrumb.Draw());
            this.Output.Write(this.ActionButtons.Draw());
            this.Output.Write("</div>");
            this.Output.Write("</div>");
        }
        public override void Dispose()
        {
            this.Dispose(true);

            this.TabControl.Dispose();
            this.TabControl = null;

            GC.SuppressFinalize(this);
        }
        protected long GetPreviousEntityID()
        {
            if (!string.IsNullOrEmpty(this.Request.GetValue("IDList")))
            {
                var IDs = this.Request.GetValue("IDList").ToLongList();
                for (int i = 0; i < IDs.Count; i++)
                {
                    if (IDs[i] == this.EntityID)
                    {
                        if (i == 0)
                            return 0;
                        else
                            return IDs[i - 1];
                    }
                }
            }
            return 0;
        }
        protected long GetNextEntityID()
        {
            if (!string.IsNullOrEmpty(this.Request.GetValue("IDList")))
            {
                var IDs = this.Request.GetValue("IDList").ToLongList();
                for (int i = 0; i < IDs.Count; i++)
                {
                    if (IDs[i] == this.EntityID)
                    {
                        if (i + 1 < IDs.Count)
                            return IDs[i + 1];
                        else
                            return 0;
                    }
                }
            }
            return 0;
        }
        public virtual bool CanDrawField(Fields.BaseField<T> field)
        {
            if (this.Configuration.ReadOnly)
                field.Editable = false;
            return true;
        }

        public virtual bool CanDrawTab(Tab<T> tab)
        {
            return true;
        }

        public virtual void Dispose(Boolean disposing)
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
            }
        }
    }
}
