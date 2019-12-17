using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Attributes
{
    public class RelationFKProperty : Attribute
    {
        public string PropertyName { get; set; }

        public RelationFKProperty()
        {
            
        }
        public RelationFKProperty(string propertyName)
        {
            this.PropertyName = propertyName;
        }
    }
}
