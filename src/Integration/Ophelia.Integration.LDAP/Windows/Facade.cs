using System.Security.Principal;

namespace Ophelia.Integration.LDAP.Windows
{
    public class Facade : ADFacade
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static WindowsIdentity GetIdentity()
        {
            return WindowsIdentity.GetCurrent();
        }
    }
}
