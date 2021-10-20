using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Integration.Documentation.BookStack.Models
{
    public class PageModel
    {
        public long book_id { get; set; }
        public long chapter_id { get; set; }
        public int priority { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string markdown { get; set; }
        public bool template { get; set; }
        public int revision_count { get; set; }
        public bool draft { get; set; }
        public string referenceCode { get; set; }
        public string html { get; set; }
        public UserModel owned_by { get; set; }
        public List<TagModel> tags { get; set; }
        public string url { get; set; }
        public PageModel()
        {
            this.tags = new List<TagModel>();
            this.markdown = "";
            this.slug = "";
            this.name = "";
            this.html = "";
        }
    }
}
