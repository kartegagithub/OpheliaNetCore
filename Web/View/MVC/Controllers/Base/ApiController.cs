using Microsoft.AspNetCore.Mvc;

namespace Ophelia.Web.View.Mvc.Controllers.Base
{
    public abstract class ApiController : ControllerBase
    {
        public ApiController()
        {

        }

        public virtual Client Client { get; protected set; }
    }
}
