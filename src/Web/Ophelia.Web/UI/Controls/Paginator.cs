using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using QueryString = Ophelia.Web.Application.Client.QueryString;

namespace Ophelia.Web.UI.Controls
{
    public class Paginator : List
    {
        private QueryString QS { get; set; }
        public int LinkedPageCount { get; set; }
        public int ItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public string NextPageTitle { get; set; }
        public string PreviousPageTitle { get; set; }
        public string EndTitle { get; set; }
        public string StartTitle { get; set; }
        public string PageKeyword { get; set; }
        public string[] ExcludedKeys { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            if (this.PageSize <= 0)
                this.PageSize = 25;
            if (this.LinkedPageCount <= 0)
                return;

            if (this.ExcludedKeys != null)
            {
                foreach (var item in this.ExcludedKeys)
                {
                    this.QS.Remove(item);
                }
            }
            var pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(this.ItemCount) / this.PageSize));
            if (pageCount <= 0)
                return;

            var leftSide = Convert.ToInt32(this.LinkedPageCount / 2);

            var startPage = this.CurrentPage - leftSide;
            if (startPage <= 0)
            {
                startPage = 1;
                leftSide = 0;
            }

            var endPage = this.CurrentPage + (this.LinkedPageCount - leftSide) - 1;
            if (endPage > pageCount)
            {
                startPage -= endPage - pageCount;
                if (startPage <= 0)
                    startPage = 1;
                endPage = pageCount;
            }
            if (pageCount > 1 && this.CurrentPage > 1)
            {
                this.QS.Update(this.PageKeyword, "1");
                this.AddItem("‹‹", this.QS.Value, "page-item", "page-link").Controls.FirstOrDefault().AddAttribute("title", this.StartTitle);
            }

            if (startPage > 1)
            {
                if (startPage - Convert.ToInt32(this.LinkedPageCount / 2) <= 0)
                    this.QS.Update(this.PageKeyword, "1");
                else
                    this.QS.Update(this.PageKeyword, (startPage - Convert.ToInt32(this.LinkedPageCount / 2)).ToString());
                this.AddItem("‹", this.QS.Value, "page-item", "page-link").Controls.FirstOrDefault().AddAttribute("title", this.PreviousPageTitle);
            }
            for (int i = startPage; i <= endPage; i++)
            {
                this.QS.Update(this.PageKeyword, i.ToString());
                this.AddItem(i.ToString(), this.QS.Value, (i == this.CurrentPage ? "active page-item" : "page-item"), "page-link");
            }
            if (endPage < pageCount)
            {
                if (endPage + Convert.ToInt32(this.LinkedPageCount / 2) + 1 > pageCount)
                    this.QS.Update(this.PageKeyword, pageCount.ToString());
                else
                    this.QS.Update(this.PageKeyword, (endPage + Convert.ToInt32(this.LinkedPageCount / 2) + 1).ToString());
                this.AddItem("›", this.QS.Value, "page-item", "page-link").Controls.FirstOrDefault().AddAttribute("title", this.NextPageTitle);
            }
            if (pageCount > 1 && this.CurrentPage < endPage)
            {
                this.QS.Update(this.PageKeyword, pageCount.ToString());
                this.AddItem("››", this.QS.Value, "page-item", "page-link").Controls.FirstOrDefault().AddAttribute("title", this.EndTitle);
            }
            base.onBeforeRenderControl(writer);
        }

        public Paginator(HttpRequest request, int itemCount, int pageSize, int currentPage, int linkedPageCount = 6, string PageKeyword = "page")
        {
            this.ItemCount = itemCount;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
            this.LinkedPageCount = linkedPageCount;
            this.QS = new QueryString(request);
            this.CssClass = "pagination";
            this.PageKeyword = PageKeyword;
        }
        public override void Dispose()
        {
            base.Dispose();
            this.QS = null;
        }
    }
}
