using System;

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
