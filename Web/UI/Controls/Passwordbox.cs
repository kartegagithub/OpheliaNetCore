using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.UI.Controls
{
    public class Passwordbox : Textbox
    {        
        public Passwordbox()
        {
            this.Type = "password";
        }
    }
}
