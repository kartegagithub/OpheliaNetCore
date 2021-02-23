using Ophelia.Web.UI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ophelia.Web.View.Mvc.Controls.Binders.EntityBinder
{
    public class TabControl<T> : Panel where T : class
    {
        public EntityBinder<T> Binder { get; private set; }
        public List<Tab<T>> Tabs { get; protected set; }
        public List TabsHeader { get; protected set; }

        public TabControl(EntityBinder<T> binder)
        {
            this.Binder = binder;
            this.TabsHeader = new List();
            this.TabsHeader.CssClass = "nav nav-tabs";

            this.CreateTabs();
            this.CssClass = "tab-content";
        }

        public virtual Tab<T> AddTab(string Title, bool IsRequired = false)
        {
            return this.AddTab(this.CreateTab(Title, IsRequired));
        }

        public virtual Tab<T> AddTab(Tab<T> tab)
        {
            tab.Visible = this.Binder.CanDrawTab(tab);
            this.Tabs.Add(tab);
            return tab;
        }

        protected virtual Tab<T> CreateTab(string title, bool IsRequired = false)
        {
            return new Tab<T>(title, this) { IsRequired = IsRequired };
        }

        protected virtual void CreateTabs()
        {
            this.Tabs = new List<Tab<T>>();
        }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.Controls.Clear();
            this.Controls.Add(this.TabsHeader);
            if (this.Tabs.Where(op => op.Visible == true).Count() <= 1)
            {
                this.TabsHeader.Visible = false;
            }

            if (!this.Tabs.Where(op => op.IsSelected == true).Any())
            {
                this.Tabs.Where(op => op.Visible == true).FirstOrDefault().IsSelected = true;
            }
            foreach (var tab in this.Tabs)
            {
                if (tab.Visible)
                {
                    var listItem = new ListItem();
                    var link = new Link();
                    link.Text = tab.Title;
                    if (tab.Callback)
                    {

                    }
                    else
                    {
                        link.AddAttribute("data-toggle", "tab");
                    }
                    listItem.Controls.Add(link);
                    if (tab.IsSelected)
                        listItem.CssClass += " in active show";
                    this.TabsHeader.Controls.Add(listItem);

                    this.Controls.Add(tab);
                }
            }
        }
        public virtual void RenderHeader()
        {
            var selectedTab = this.Tabs.Where(op => op.ID == this.Binder.Client.Request.GetValue("tab")).FirstOrDefault();
            if (selectedTab == null)
                selectedTab = this.Tabs.Where(op => op.Visible == true).FirstOrDefault();
            if (selectedTab != null)
                selectedTab.IsSelected = true;

            if (!this.Binder.Configuration.HideTabHeader)
            {
                if (this.Tabs.Where(op => op.Visible == true).Count() > 1)
                {
                    this.Binder.Output.Write("<ul class=\"nav nav-tabs\">");
                    foreach (var tab in this.Tabs)
                    {
                        if (!tab.Visible || !this.Binder.CanDrawTab(tab))
                            continue;

                        this.Binder.Output.Write("<li");
                        if (tab.IsSelected)
                            this.Binder.Output.Write(" class='active show'");
                        this.Binder.Output.Write(">");
                        this.Binder.Output.Write("<a data-toggle=\"tab\" href=\"#" + (this.Binder.IsAjaxEntityBinderRequest ? "AjaxBinder" : "") + tab.ID + "\">" + tab.Title + "</a>");
                        this.Binder.Output.Write("</li>");
                    }
                    this.Binder.Output.Write("</ul>");
                }
            }
            this.Binder.Output.Write("<div class='tab-content'>");
        }

        public virtual void RenderFooter()
        {
            this.Binder.Output.Write("</div>"); /* Tab-Content */
        }

        public override void Dispose()
        {
            base.Dispose();

            this.Tabs.Clear();
            this.Tabs = null;
        }
    }
}
