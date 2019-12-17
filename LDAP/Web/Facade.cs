using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Claims;

namespace Ophelia.LDAP.Web
{
    public class Facade : ADFacade
    {
        public static ClaimsPrincipal GetIdentity()
        {
            return Ophelia.Web.Client.Current.Context.User;
        }
    }
}
