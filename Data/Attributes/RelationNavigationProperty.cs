using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Attributes
{
    public class RelationNavigationProperty : Attribute
    {
        public string PropertyName { get; set; }
    }
}
