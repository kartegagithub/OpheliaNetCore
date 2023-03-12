using System;

namespace Ophelia.Data.Attributes
{
    public class RelationNavigationProperty : Attribute
    {
        public string PropertyName { get; set; }
    }
}
