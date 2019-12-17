using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;
using Ophelia.Web.View.Mvc.Controls.Binders;
using System.Linq.Expressions;
using System.IO;
using Ophelia.Web.View.Mvc.Controls.Binders.Fields;

namespace Ophelia.Web.View.Mvc.Controls.Binders.EntityBinder
{
    public class Tab<T> : FieldContainer<T> where T : class
    {
        public bool IsSelected { get; set; }
        public bool IsRequired { get; set; }
        public string Title { get; set; }
        public bool Callback { get; set; }
        public TabControl<T> TabControl { get; private set; }
        public override TextWriter Output { get { return this.TabControl.Binder.Output; } set { } }

        public override T Entity
        {
            get
            {
                return this.TabControl.Binder.Entity;
            }
        }

        public override Client Client
        {
            get
            {
                return this.TabControl.Binder.Client;
            }
        }

        public override string[] DefaultEntityProperties
        {
            get
            {
                return this.TabControl.Binder.DefaultEntityProperties;
            }
        }

        public override int CurrentLanguageID
        {
            get
            {
                return this.TabControl.Binder.CurrentLanguageID;
            }
        }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.CssClass += " " + this.TabControl.Binder.Configuration.TabPaneCssClass;
            if (this.IsSelected)
                this.CssClass += " in active";
        }

        public virtual void RenderHeader()
        {
            this.Output.Write("<div class='" + this.TabControl.Binder.Configuration.TabPaneCssClass);
            if (this.IsSelected)
                this.Output.Write(" in active");
            this.Output.Write("' id=\"" + (this.TabControl.Binder.IsAjaxEntityBinderRequest ? "AjaxBinder" : "") + this.ID + "\">");
        }

        public virtual void RenderFooter()
        {
            this.Output.Write("</div>");
        }
        protected virtual void OnBeforeAddField(BaseField<T> field)
        {

        }
        protected virtual void OnAfterAddField(BaseField<T> field)
        {

        }
        public override BaseField<T> AddField(BaseField<T> field)
        {
            this.OnBeforeAddField(field);
            field.SetDataValue();
            field.Visible = this.CanDrawField(field);
            if (field.Visible && this.TabControl.Binder.Configuration.AllowModeChange && !string.IsNullOrEmpty(this.TabControl.Binder.Configuration.InitialMode))
            {
                var mode = this.TabControl.Binder.ModeFields.Count > 0 ? this.TabControl.Binder.ModeFields[this.TabControl.Binder.Configuration.InitialMode] : null;
                var modeText = this.TabControl.Binder.ModeTextFields.Count > 0 ? this.TabControl.Binder.ModeTextFields[this.TabControl.Binder.Configuration.InitialMode] : null;
                if (mode != null)
                {
                    if (field.Expression != null)
                    {
                        if (!mode.Where(op => op.Body.ParsePath() == field.Expression.Body.ParsePath()).Any())
                            field.Style.Add("display", "none");
                    }
                    else if (field.Expression == null && modeText != null)
                    {
                        if (!modeText.Contains(field.Text))
                            field.Style.Add("display", "none");
                    }
                    else
                        field.Style.Add("display", "none");
                }
            }
            if (this.TabControl.Binder.Configuration.Help.Enable)
            {
                if (field.Expression != null)
                {
                    var fieldName = field.Expression.Body.ParsePath();
                    field.SearchHelp = this.TabControl.Binder.Configuration.Help.SearchHelps.Where(op => op.Path == fieldName).FirstOrDefault();
                    if (this.TabControl.Binder.Configuration.Help.Documentation.ContainsKey(fieldName) && !this.TabControl.Binder.Configuration.Help.DiscardedDocumentation.Contains(fieldName))
                    {
                        if (this.TabControl.Binder.Configuration.Help.Documentation.ContainsKey(fieldName))
                        {
                            field.HelpTip = this.TabControl.Binder.Configuration.Help.Documentation[fieldName];
                        }
                        field.HelpNavigator = typeof(T).FullName + "/" + fieldName;
                        field.HelpClassName = this.TabControl.Binder.Configuration.Help.ClassName;
                    }
                }
                else
                {
                    field.SearchHelp = this.TabControl.Binder.Configuration.Help.SearchHelps.Where(op => op.Path == field.Text).FirstOrDefault();
                    if (this.TabControl.Binder.Configuration.Help.Documentation.ContainsKey(field.Text) && !this.TabControl.Binder.Configuration.Help.DiscardedDocumentation.Contains(field.Text))
                    {
                        if (this.TabControl.Binder.Configuration.Help.Documentation.ContainsKey(field.Text))
                            field.HelpTip = this.TabControl.Binder.Configuration.Help.Documentation[field.Text];
                        field.HelpNavigator = typeof(T).FullName + "/" + field.Text;
                        field.HelpClassName = this.TabControl.Binder.Configuration.Help.ClassName;
                    }
                }
            }
            this.Controls.Add(field);
            this.OnAfterAddField(field);
            return field;
        }

        public override bool CanDrawField(BaseField<T> field)
        {
            return this.TabControl.Binder.CanDrawField(field);
        }

        public Tab(string Title, TabControl<T> TabControl)
        {
            this.TabControl = TabControl;
            this.ID = Title + "-tab";
            this.Title = this.TabControl.Binder.Client.TranslateText(Title);
            this.CssClass = "";
            this.Visible = true;
            this.LabelCssClass = this.TabControl.Binder.Configuration.LabelCssClass;
            this.DataControlParentCssClass = this.TabControl.Binder.Configuration.DataControlParentCssClass;
        }
    }
}
