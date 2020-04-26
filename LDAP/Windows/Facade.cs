using System.Security.Principal;

namespace Ophelia.LDAP.Windows
{
    public class Facade : ADFacade
    {
        public static WindowsIdentity GetIdentity()
        {
            return WindowsIdentity.GetCurrent();
        }
    }
}
