using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace Ophelia
{
    /// <summary>
    /// 
    /// </summary>
    public static class IHtmlContentExtensions
    {
        public static string GetString(this IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
