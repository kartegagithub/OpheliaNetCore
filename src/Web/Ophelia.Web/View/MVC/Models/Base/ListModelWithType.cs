using Ophelia.Service;
using Ophelia.Web.Service;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public Func<string, WebApiCollectionRequest<T>, ServiceCollectionResult<T>> RemoteDataSource { get; set; }
        public Func<IQueryable<T>, IQueryable<T>> OnBeforeQueryExecuted { get; set; }
        public Func<Service.WebApiCollectionRequest<T>, Service.WebApiCollectionRequest<T>> OnBeforeRemoteDataSourceCall { get; set; }
        public bool DataImportPreview { get; set; }
        public bool ParentDrawsLayout { get; set; }
        public string DataImportKey { get; set; }
        public override void Dispose()
        {
            base.Dispose();
            if (this.Items != null)
            {
                this.Items.Clear();
                this.Items = null;
            }
            if (this.Query != null)
            {
                this.Query = null;
            }
        }
    }
}
