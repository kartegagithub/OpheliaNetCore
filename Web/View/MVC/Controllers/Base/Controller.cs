using System.Threading;
namespace Ophelia.Web.View.Mvc.Controllers.Base
{
    public abstract class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public Controller()
        {
            
        }
        public virtual Client Client { get; protected set; }
    }
}
