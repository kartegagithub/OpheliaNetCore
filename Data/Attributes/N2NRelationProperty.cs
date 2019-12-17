using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Attributes
{
    public class N2NRelationProperty : Attribute
    {
        public string PropertyName { get; set; }
        public string FilterName { get; set; }
        public object FilterValue { get; set; }
        public string ReverseFilterName { get; set; }
        public Type RelationClassType { get; set; }

        public N2NRelationProperty()
        {
            
        }
    }
}
