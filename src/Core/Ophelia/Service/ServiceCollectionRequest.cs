using System.Collections.Generic;

namespace Ophelia.Service
{
    public class ServiceCollectionRequest<T> : ServiceObjectRequest<T>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public new ServiceCollectionRequest<T> AddParam(string key, object value)
        {
            return (ServiceCollectionRequest<T>)base.AddParam(key, value);
        }
        public ServiceCollectionRequest()
        {
            this.Parameters = new Dictionary<string, object>();
            this.Page = 1;
            this.PageSize = 25;
        }
    }
}
