using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NJsonSchema.Annotations;
using NSwag.Annotations;
using System.Collections.Generic;

namespace Ophelia.Web.Service
{
    public class WebApiCollectionRequest<T> : Ophelia.Service.ServiceCollectionRequest<T>
    {
        [OpenApiIgnore]
        [JsonSchemaIgnore]
        [ValidateNever]
        public Data.Querying.Query.QueryData? QueryData { get; set; }

        [ValidateNever]
        public override T Data { get => base.Data; set => base.Data = value; }

        public new WebApiCollectionRequest<T> AddParam(string key, object value)
        {
            return (WebApiCollectionRequest<T>)base.AddParam(key, value);
        }
    }
}
