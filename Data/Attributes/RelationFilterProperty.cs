using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Attributes
{
    public class RelationFilterProperty: Attribute
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }
}
