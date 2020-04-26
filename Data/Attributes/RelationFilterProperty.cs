using System;

namespace Ophelia.Data.Attributes
{
    public class RelationFilterProperty : Attribute
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }
}
