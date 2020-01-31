using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ophelia.Web.View.Mvc.Models;
using Ophelia.Web.Application.Client;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

namespace Ophelia.Web.View.Mvc.Html
{
    public static class PaginationExtensions
    {
        public static HtmlString PaginationFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, PaginationModel>> expression)
        {
            return htmlHelper.PaginationFor(expression, PaginationSize.Default, PaginationAlignment.Centered);
        }

        public static HtmlString PaginationFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, PaginationModel>> expression, PaginationSize size, PaginationAlignment align)
        {
            var className = GenerateClassName(size, align);
            var listHtmlAttributes = htmlHelper.AnonymousObjectToHtmlAttributes(new { @class = className });
            return htmlHelper.PaginationInternal(expression, listHtmlAttributes);
        }

        internal static HtmlString PaginationInternal<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, PaginationModel>> expression, IDictionary<string, object> htmlAttributes)
        {
            var model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            if (model == null)
                return HtmlString.Empty;

            var routeValues = htmlHelper.ViewContext.RouteData.Values;
            model.EnsurePageCount();

            if (model.PageCount > 1)
            {
                var listBuilder = new TagBuilder("ul");
                if (model.PageNumber > 1)
                {
                    listBuilder.InnerHtml.AppendHtml(PageLinkInternal(htmlHelper, routeValues, model.PageNumber, -1, model.PageNumber <= 1, false, model));
                }
                for (int index = model.FirstPage; index < model.LastPage; index++)
                {
                    listBuilder.InnerHtml.AppendHtml(PageLinkInternal(htmlHelper, routeValues, index, 0, false, index == model.PageNumber, model));
                }
                if (model.PageNumber < model.PageCount)
                {
                    listBuilder.InnerHtml.AppendHtml(PageLinkInternal(htmlHelper, routeValues, model.PageNumber, 1, model.PageNumber >= model.PageCount, false, model));
                }

                var containerBuilder = new TagBuilder("div");
                containerBuilder.InnerHtml.AppendHtml(listBuilder.InnerHtml.GetString());
                containerBuilder.MergeAttribute("name", expression.ParsePath());
                containerBuilder.MergeAttribute("id", expression.ParsePath());
                containerBuilder.MergeAttributes(htmlAttributes);
                return new HtmlString(containerBuilder.InnerHtml.GetString());
            }
            return new HtmlString(string.Empty);
        }

        internal static string PageLinkInternal(IHtmlHelper htmlHelper, RouteValueDictionary routeValues, int pageNumber, int move, bool disabled, bool active, PaginationModel PaginationModel = null)
        {
            var model = htmlHelper.ViewData.Model;
            var cssClass = disabled ? "disabled" : string.Empty;
            cssClass = active ? "active" : cssClass;
            var pageLinkBuilder = new TagBuilder("li");
            var linkText = pageNumber.ToString();
            if (move != 0)
            {
                pageNumber += move;
                linkText = (move == -1) ? "«" : "»";
            }

            var routeData = new RouteValueDictionary(routeValues);
            routeData["page"] = pageNumber;
            var queryString = htmlHelper.ViewContext.HttpContext.Request.Query;
            foreach (string key in queryString.Keys)
            {
                if (!routeData.ContainsKey(key))
                    routeData.Add(key, queryString[key]);
            }

            string LinkUrl = "javascript:void(0);";
            if (!active)
            {

                LinkUrl = "/" + routeData["Controller"] + "/" + routeData["Action"] + "/" + routeData["page"];
                if (!string.IsNullOrEmpty(queryString.ToString()))
                {
                    LinkUrl += "?" + queryString;
                }
            }

            if (PaginationModel != null && PaginationModel.DrawMode == 1)
            {
                string IsSelected = active ? "a" : "c";
                return DrawPageNumberForJQueryMobile(linkText, LinkUrl, IsSelected);
            }
            else
            {
                pageLinkBuilder.InnerHtml.AppendHtml("<a href='" + LinkUrl + "' class='" + cssClass + "'>" + linkText + "</a>");
            }
            return pageLinkBuilder.InnerHtml.GetString();
        }

        private static string DrawPageNumberForJQueryMobile(string Value, string Url, string IsSelected = "c")
        {
            StringBuilder PageNumberHtml = new StringBuilder();
            PageNumberHtml.AppendLine("<a data-inline='true' data-theme='" + IsSelected + "' data-role='button' href='" + Url + "' data-corners='true' data-shadow='true' data-iconshadow='true'");
            PageNumberHtml.AppendLine(" data-wrapperels='span' class='ui-btn ui-shadow ui-btn-corner-all ui-btn-inline ui-btn-up-" + IsSelected + "'>");
            PageNumberHtml.AppendLine("<span class='ui-btn-inner'><span class='ui-btn-text'>" + Value + "</span></span></a>");
            return PageNumberHtml.ToString();
        }

        private static string GenerateClassName(PaginationSize size, PaginationAlignment align)
        {
            var className = "pagination";
            switch (size)
            {
                case PaginationSize.Large:
                    className += " pagination-large";
                    break;
                case PaginationSize.Small:
                    className += " pagination-small";
                    break;
                case PaginationSize.Mini:
                    className += " pagination-mini";
                    break;
            }

            switch (align)
            {
                case PaginationAlignment.Centered:
                    className += " pagination-centered";
                    break;
                case PaginationAlignment.Right:
                    className += " pagination-right";
                    break;
            }

            return className;
        }
    }

    public static class CustomPaginationExtensions
    {
        public static IHtmlContent CustomPaginationFor<TModel>(this IHtmlHelper<TModel> htmlHelper, long ItemCount, int pageNumber, int pageSize, List<string> excludedKeys = null)
        {
            var model = new PaginationModel()
            {
                ItemCount = ItemCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var className = CustomGenerateClassName(PaginationSize.Default, PaginationAlignment.Centered);
            var listHtmlAttributes = htmlHelper.AnonymousObjectToHtmlAttributes(new { @class = className });
            return htmlHelper.CustomPaginationInternal(model, listHtmlAttributes, excludedKeys);
        }

        public static IHtmlContent CustomPaginationFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, PaginationModel>> expression, List<string> excludedKeys = null)
        {
            return htmlHelper.CustomPaginationFor(expression, PaginationSize.Default, PaginationAlignment.Centered, excludedKeys);
        }

        public static IHtmlContent CustomPaginationFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, PaginationModel>> expression, PaginationSize size, PaginationAlignment align, List<string> excludedKeys = null)
        {
            var className = CustomGenerateClassName(size, align);
            var listHtmlAttributes = htmlHelper.AnonymousObjectToHtmlAttributes(new { @class = className });
            return htmlHelper.CustomPaginationInternal(expression, listHtmlAttributes, excludedKeys);
        }

        internal static IHtmlContent CustomPaginationInternal<TModel>(this IHtmlHelper<TModel> htmlHelper, PaginationModel model, IDictionary<string, object> htmlAttributes, List<string> excludedKeys = null, string ID = "Pagination")
        {
            if (model == null)
                return HtmlString.Empty;

            var routeValues = htmlHelper.ViewContext.RouteData.Values;
            model.EnsurePageCount();

            var QueryString = new QueryString(htmlHelper.ViewContext.HttpContext.Request);
            if (excludedKeys != null)
            {
                foreach (var key in excludedKeys)
                {
                    QueryString.Remove(key);
                }
            }
            if (model.PageCount > 1)
            {
                var listBuilder = new TagBuilder("ul");

                if (model.FirstPage > 1)
                {
                    //  if (model.FirstPage > 2)
                    listBuilder.InnerHtml.AppendHtml(CustomPageLinkInternal(htmlHelper, QueryString, 1, Button.First, false, false, model));
                    listBuilder.InnerHtml.AppendHtml(CustomPageLinkInternal(htmlHelper, QueryString, model.PageNumber - 1, Button.Previous, false, false, model));
                }

                for (int index = model.FirstPage; index <= model.LastPage; index++)
                {
                    listBuilder.InnerHtml.AppendHtml(CustomPageLinkInternal(htmlHelper, QueryString, index, Button.Number, false, index == model.PageNumber, model));
                }

                if (model.LastPage < model.PageCount)
                {

                    listBuilder.InnerHtml.AppendHtml(CustomPageLinkInternal(htmlHelper, QueryString, model.PageNumber + 1, Button.Next, false, false, model));
                    // if (model.LastPage + 1 < model.PageCount)
                    listBuilder.InnerHtml.AppendHtml(CustomPageLinkInternal(htmlHelper, QueryString, Convert.ToInt32(model.PageCount), Button.Last, false, false, model));
                }
                var containerBuilder = new TagBuilder("div");
                containerBuilder.InnerHtml.AppendHtml(listBuilder.InnerHtml.GetString());
                containerBuilder.MergeAttribute("name", ID);
                containerBuilder.MergeAttribute("id", ID);
                containerBuilder.MergeAttributes(htmlAttributes);
                return containerBuilder;
            }
            return htmlHelper.Raw(string.Empty);
        }
        internal static IHtmlContent CustomPaginationInternal<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, PaginationModel>> expression, IDictionary<string, object> htmlAttributes, List<string> excludedKeys = null)
        {
            var model = expression.Compile().Invoke(htmlHelper.ViewData.Model);
            return CustomPaginationInternal(htmlHelper, model, htmlAttributes, excludedKeys, expression.ParsePath());
        }

        internal static string CustomPageLinkInternal(IHtmlHelper htmlHelper, QueryString QS, int pageNumber, Button button, bool disabled, bool active, PaginationModel paginationModel = null)
        {
            var model = htmlHelper.ViewData.Model;
            var cssClass = disabled ? "disabled" : string.Empty;
            cssClass = active ? "active" : cssClass;
            var pageLinkBuilder = new TagBuilder("li");
            var linkText = pageNumber.ToString();
            if (button != Button.Number)
            {

                switch (button)
                {
                    case Button.First: linkText = "«"; break;
                    case Button.Previous: linkText = "‹"; break;
                    case Button.Next: linkText = "›"; break;
                    case Button.Last: linkText = "»"; break;
                }
            }

            string LinkUrl = "javascript:void(0);";
            if (!active)
            {
                QS.Update("page", pageNumber.ToString());
                LinkUrl = QS.Value;
            }

            if (paginationModel != null && paginationModel.DrawMode == 1)
            {
                string IsSelected = active ? "a" : "c";
                return CustomDrawPageNumberForJQueryMobile(linkText, LinkUrl, IsSelected);
            }
            else
            {
                pageLinkBuilder.InnerHtml.AppendHtml("<a href='" + LinkUrl + "' class='" + cssClass + "'>" + linkText + "</a>");
            }
            return pageLinkBuilder.InnerHtml.GetString();
        }

        private static string CustomDrawPageNumberForJQueryMobile(string Value, string Url, string IsSelected = "c")
        {
            StringBuilder PageNumberHtml = new StringBuilder();
            PageNumberHtml.AppendLine("<a data-inline='true' data-theme='" + IsSelected + "' data-role='button' href='" + Url + "' data-corners='true' data-shadow='true' data-iconshadow='true'");
            PageNumberHtml.AppendLine(" data-wrapperels='span' class='ui-btn ui-shadow ui-btn-corner-all ui-btn-inline ui-btn-up-" + IsSelected + "'>");
            PageNumberHtml.AppendLine("<span class='ui-btn-inner'><span class='ui-btn-text'>" + Value + "</span></span></a>");
            return PageNumberHtml.ToString();
        }

        private static string CustomGenerateClassName(PaginationSize size, PaginationAlignment align)
        {
            var className = "pagination";
            switch (size)
            {
                case PaginationSize.Large:
                    className += " pagination-large";
                    break;
                case PaginationSize.Small:
                    className += " pagination-small";
                    break;
                case PaginationSize.Mini:
                    className += " pagination-mini";
                    break;
            }

            switch (align)
            {
                case PaginationAlignment.Centered:
                    className += " pagination-centered";
                    break;
                case PaginationAlignment.Right:
                    className += " pagination-right";
                    break;
            }

            return className;
        }
    }
}
