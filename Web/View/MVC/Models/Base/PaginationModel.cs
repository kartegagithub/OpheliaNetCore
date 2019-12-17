using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Models
{
    public class PaginationModel
    {
        public bool Paging { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public long PageCount { get; private set; }
        public long ItemCount { get; set; }
        public int FirstPage { get; private set; }
        public int LastPage { get; private set; }
        public int LinkedPageCount { get; private set; }
        public byte DrawMode { get; set; }
        public string PageKey { get; set; }
        public PaginationModel()
        {
            PageNumber = 1;
            PageSize = 25;
            FirstPage = 1;
            LastPage = 1;
            DrawMode = 0;
            PageKey = "page";
            LinkedPageCount = 6;
        }

        public void EnsurePageCount()
        {
            //if page number greater than page count, set page number to 1.
            if ((this.PageNumber - 1) * this.PageSize > this.ItemCount)
                this.PageNumber = 1;

            // Set first number of page, if result less then zero, set 1.
            var start = this.PageNumber - 2;
            start = (start < 1) ? 1 : start;

            var pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(this.ItemCount) / this.PageSize));

            // Set last number of page, if count of page less then value, 
            var finish = this.PageNumber + 3;
            finish = (finish > pageCount + 1) ? pageCount + 1 : finish;

            if (start > pageCount - 4) { start = pageCount - 4; start = (start < 1) ? 1 : start; }
            if (finish < 6 && pageCount > 5) { finish = 6; }
            if (finish > pageCount) { finish = pageCount; }

            this.FirstPage = start;
            this.LastPage = finish;
            this.PageCount = pageCount;
        }
    }
}
