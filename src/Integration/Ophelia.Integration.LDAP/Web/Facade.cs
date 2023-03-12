using System.Security.Claims;

namespace Ophelia.Integration.LDAP.Web
{
    public class Facade : ADFacade
    {
        public static ClaimsPrincipal GetIdentity()
        {
            return Ophelia.Web.Client.Current.Context.User;
        }
    }
}
