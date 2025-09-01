using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Ophelia.Web.Service
{
    public class WebApiObjectRequest<T> : Ophelia.Service.ServiceObjectRequest<T>
    {
        [ValidateNever]
        public override T Data { get => base.Data; set => base.Data = value; }
    }
}
