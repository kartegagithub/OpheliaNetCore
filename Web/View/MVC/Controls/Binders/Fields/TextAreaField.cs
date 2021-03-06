﻿using Ophelia.Web.UI.Controls;
using System;
using System.IO;

namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class TextAreaField<T> : BaseField<T> where T : class
    {
        public new TextArea DataControl { get { return (TextArea)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new TextArea();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            if (this.ExpressionValue != null)
                this.DataControl.Value = Convert.ToString(this.ExpressionValue);
            this.HasValue = !string.IsNullOrEmpty(this.DataControl.Value);
        }
        public TextAreaField(FieldContainer<T> FieldContainer) : base(FieldContainer)
        {

        }
    }
}
