using System;
using System.Runtime.Serialization;

namespace Ophelia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StringValueAttribute : Attribute
    {
        public string StringValue { get; protected set; }

        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }
}
