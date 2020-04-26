﻿using Ophelia.Web.UI.Controls;
using System;
using System.IO;


namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class TextboxField<T> : BaseField<T> where T : class
    {
        public new Textbox DataControl { get { return (Textbox)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new Textbox();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            if (this.ExpressionValue != null)
                this.DataControl.Value = Convert.ToString(this.ExpressionValue);
            this.HasValue = !string.IsNullOrEmpty(this.DataControl.Value);
        }
        public TextboxField(FieldContainer<T> FieldContainer) : base(FieldContainer)
        {

        }
    }
}
