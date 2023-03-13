using NJsonSchema.Annotations;
using NSwag.Annotations;
using System.Collections.Generic;

namespace Ophelia.Web.Service
{
    public class WebApiCollectionRequest<T> : Ophelia.Service.ServiceCollectionRequest<T>
    {
        [OpenApiIgnore]
        [JsonSchemaIgnore]
        public Data.Querying.Query.QueryData QueryData { get; set; }

        public new WebApiCollectionRequest<T> AddParam(string key, object value)
        {
            return (WebApiCollectionRequest<T>)base.AddParam(key, value);
        }
    }
}
