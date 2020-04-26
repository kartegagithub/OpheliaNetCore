using Microsoft.AspNetCore.Html;
using Ophelia.Web.UI.Controls;

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
