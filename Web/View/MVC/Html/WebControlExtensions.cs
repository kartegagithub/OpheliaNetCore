using Ophelia.Web.UI.Controls;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Html
{
    public static class WebControlExtensions
    {
        public static HtmlString Render(this WebControl control)
        {
            return new HtmlString(control.Draw());
        }
    }
}
