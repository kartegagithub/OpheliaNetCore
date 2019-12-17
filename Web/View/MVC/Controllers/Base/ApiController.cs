using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Web;
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
