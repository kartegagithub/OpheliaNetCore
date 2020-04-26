using System.Collections.Generic;

namespace Ophelia.Service
{
    public class WebApiCollectionRequest<T> : WebApiObjectRequest<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public Data.Querying.Query.QueryData QueryData { get; set; }
        public new WebApiCollectionRequest<T> AddParam(string key, object value)
        {
            return (WebApiCollectionRequest<T>)base.AddParam(key, value);
        }
        public WebApiCollectionRequest()
        {
            this.Parameters = new Dictionary<string, object>();
        }
    }
}
