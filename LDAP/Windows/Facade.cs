using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace Ophelia.LDAP.Windows
{
    public class Facade:ADFacade
    {
        public static WindowsIdentity GetIdentity()
        {
            return WindowsIdentity.GetCurrent();
        }
    }
}
