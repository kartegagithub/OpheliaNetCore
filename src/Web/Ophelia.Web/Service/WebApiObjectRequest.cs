using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NJsonSchema.Annotations;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophelia.Web.Service
{
    public class WebApiObjectRequest<T> : Ophelia.Service.ServiceObjectRequest<T>
    {
        [ValidateNever]
        public override T Data { get => base.Data; set => base.Data = value; }
    }
}
