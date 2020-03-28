using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.UI.Controls
{
    public class WebControl : IDisposable
    {
        private string sID;
        public virtual string ID
        {
            get
            {
                return this.sID;
            }

            set
            {
                if (value.IndexOf(".") > -1)
                    this.sID = value.Replace(".", "_");
                else
                    this.sID = value;
            }
        }
        public virtual string Name { get; set; }
        public virtual string TagName { get; set; }
        public virtual object HtmlAttributes { get; set; }
        public virtual Dictionary<string, string> Attributes { get; set; }
        public virtual Dictionary<string, string> Style { get; set; }
        public virtual List<WebControl> Controls { get; set; }
        public virtual bool IsHidden { get; set; }
        public string CssClass { get; set; }
        public virtual TextWriter Output { get; set; }
        public bool Visible { get; set; }
        public virtual Dictionary<string, string> EventHandlers { get; private set; }
        public virtual void AddEvent(string eventName, string functionName)
        {
            if (this.EventHandlers == null)
                this.EventHandlers = new Dictionary<string, string>();
            this.EventHandlers[eventName] = functionName;
        }

        public virtual string Draw()
        {
            StringBuilder sb = new StringBuilder();
            this.Output = new System.IO.StringWriter(sb, System.Globalization.CultureInfo.InvariantCulture);
            this.RenderControl();
            return sb.ToString();
        }
        public virtual void RenderControl()
        {
            this.RenderControlAsText(this.Output);
        }

        protected virtual void onBeforeRenderControl(TextWriter writer)
        {

        }
        protected virtual void onRenderControl(TextWriter writer)
        {

        }
        protected virtual void onAfterRenderControl(TextWriter writer)
        {

        }
        public virtual void RenderControlAsText(TextWriter writer)
        {
            this.RenderControl(writer);
        }
        protected virtual void RenderContents(TextWriter writer)
        {
            
        }
        public virtual void RenderControl(TextWriter writer)
        {
            if (!this.Visible)
                return;

            if (this.HtmlAttributes != null)
            {
                var type = this.HtmlAttributes.GetType();
                var props = type.GetProperties().ToDictionary(op => op.Name, op => op.GetValue(this.HtmlAttributes, null));
                foreach (var item in props)
                {
                    this.AddAttribute(item.Key, Convert.ToString(item.Value));
                }
            }
            if (!string.IsNullOrEmpty(this.Name))
                this.AddAttribute("name", this.Name);
            if (this.EventHandlers != null && this.EventHandlers.Count > 0)
            {
                foreach (var item in this.EventHandlers)
                {
                    this.AddAttribute(item.Key, item.Value);
                }
            }
            if (this.IsHidden)
            {
                this.CssClass += " hidden";
            }
            this.onBeforeRenderControl(writer);
            this.onRenderControl(writer);
            this.RenderInternal(writer);
            this.onAfterRenderControl(writer);
        }
        private void RenderInternal(TextWriter writer)
        {
            writer.Write("<");
            writer.Write(this.TagName);
            writer.Write(" ");
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                if (this.Attributes.ContainsKey("class"))
                    this.Attributes["class"] += " " + this.CssClass;
                else
                    this.Attributes["class"] = this.CssClass;
            }
            if (!string.IsNullOrEmpty(this.ID))
                this.AddAttribute("id", this.ID);

            foreach (var attr in this.Attributes)
            {
                writer.Write(" ");
                writer.Write(attr.Key);
                writer.Write("=");
                writer.Write("\"");
                writer.Write(attr.Value);
                writer.Write("\" ");
            }
            if (this.Style.Count > 0)
            {
                writer.Write(" style=\"");
                foreach (var style in this.Style)
                {
                    writer.Write(style.Key);
                    writer.Write(":");
                    writer.Write(style.Value);
                    writer.Write(";");
                }
                writer.Write("\" ");
            }
            writer.Write(">");
            foreach (var control in this.Controls)
            {
                control.RenderControl(writer);
            }
            this.RenderContents(writer);
            writer.Write("</");
            writer.Write(this.TagName);
            writer.Write(">");
        }
        public void AddAttribute(string key, string value)
        {
            if (!this.Attributes.ContainsKey(key))
                this.Attributes.Add(key, value);
            else
                this.Attributes[key] = value;
        }
        public virtual void Dispose()
        {

        }

        public WebControl()
        {
            this.Visible = true;
        }
        public WebControl(TextWriterTag tag) : this(tag.ToString().ToLower().Replace("ı", "i"))
        {

        }
        public WebControl(string tag) : this()
        {
            this.TagName = tag;
            this.Controls = new List<WebControl>();
            this.Attributes = new Dictionary<string, string>();
            this.Style = new Dictionary<string, string>();
        }
    }
}
