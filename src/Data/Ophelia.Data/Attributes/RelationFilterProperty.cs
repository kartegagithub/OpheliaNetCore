using System;

namespace Ophelia.Data.Attributes
{
    public class RelationFilterProperty : Attribute
    {
        public string PropertyName { get; set; }
        public Comparison Comparison { get; set; }
        public object Value { get; set; }
        public RelationFilterProperty()
        {
            this.Comparison = Comparison.Equal;
        }
    }
}
