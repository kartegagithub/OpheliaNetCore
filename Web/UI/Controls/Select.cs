using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace Ophelia.Web.UI.Controls
{
    public class Select : WebControl
    {
        public IEnumerable DataSource { get; set; }
        public string SelectedValue { get; set; }
        public string IndentationMemberName { get; set; }
        public string DisplayMemberName { get; set; }
        public string ValueMemberName { get; set; }
        public string DefaultText { get; set; }
        public string DefaultValue { get; set; }
        public bool IsMultiple { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.Controls.Clear();
            if (this.IsMultiple)
            {
                this.AddAttribute("multiple", "true");
            }
            if (!string.IsNullOrEmpty(this.DefaultText) || !string.IsNullOrEmpty(this.DefaultValue))
            {
                this.Controls.Add(new Option() { Text = this.DefaultText, Value = this.DefaultValue });
            }
            if (this.DataSource != null)
            {
                var accessor = new Ophelia.Reflection.Accessor();
                Option option = null;
                foreach (var item in this.DataSource)
                {
                    if (item.GetType().FullName == "System.Web.Mvc.SelectListItem" || item.GetType().FullName == "Microsoft.AspNetCore.Mvc.Rendering.SelectListItem")
                    {
                        this.DisplayMemberName = "Text";
                        this.ValueMemberName = "Value";
                    }

                    option = new Option();

                    accessor.Item = item;

                    int indentation = 0;
                    if (!string.IsNullOrEmpty(this.IndentationMemberName))
                    {
                        accessor.MemberName = this.IndentationMemberName;
                        int.TryParse(Convert.ToString(accessor.Value), out indentation);
                    }
                    accessor.MemberName = this.DisplayMemberName;
                    option.Text = Convert.ToString(accessor.Value);

                    var blanks = "";
                    for (int i = 1; i < indentation; i++)
                    {
                        blanks += "&nbsp;&nbsp;&nbsp;&nbsp;";
                    }
                    option.Text = blanks + option.Text;

                    accessor.MemberName = this.ValueMemberName;
                    option.Value = Convert.ToString(accessor.Value);

                    if (!string.IsNullOrEmpty(this.SelectedValue) && this.SelectedValue.IndexOf(",") > -1)
                    {
                        option.IsSelected = this.SelectedValue.Split(',').Where(op => op.Equals(option.Value)).Any();
                    }
                    else
                        option.IsSelected = this.SelectedValue == option.Value;

                    this.Controls.Add(option);

                    option = null;
                }
                accessor = null;
            }
        }
        public Select() : base(TextWriterTag.Select)
        {

        }
    }

    public class Option : WebControl
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool IsSelected { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            this.AddAttribute("value", this.Value);
            if (this.IsSelected)
            {
                this.AddAttribute("selected", "selected");
            }
            base.onBeforeRenderControl(writer);
        }
        protected override void RenderContents(TextWriter writer)
        {
            writer.Write(this.Text);
            base.RenderContents(writer);
        }
        public Option() : base(TextWriterTag.Option)
        {

        }
    }
}
