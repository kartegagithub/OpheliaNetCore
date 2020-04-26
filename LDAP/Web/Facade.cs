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
