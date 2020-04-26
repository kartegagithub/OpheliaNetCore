using System;
using System.Collections.Generic;

namespace Ophelia.Data.Attributes
{
    public class ExcludeDefaultColumn : Attribute
    {
        public List<string> Columns { get; set; }
        public ExcludeDefaultColumn(string Column)
        {
            this.Columns = new List<string>();
            this.Columns.Add(Column);
        }
        public ExcludeDefaultColumn(string[] columns)
        {
            this.Columns = new List<string>();
            this.Columns.AddRange(columns);
        }
    }
}
