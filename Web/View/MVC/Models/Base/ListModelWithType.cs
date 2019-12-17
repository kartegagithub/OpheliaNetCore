using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Models
{
    public class ListModel<T> : ListModel
    {
        public ListModel()
        {
            this.Items = new List<T>();
        }
        public List<T> Items { get; set; }
        public IQueryable<T> Query { get; set; }
        public bool DataImportPreview { get; set; }
        public string DataImportKey { get; set; }
        public override void Dispose()
        {
            base.Dispose();
            if (this.Items != null)
            {
                this.Items.Clear();
                this.Items = null;
            }
            if(this.Query != null)
            {
                this.Query = null;
            }
        }
    }
}
