using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Querying.Query
{
    public static class QueryExtensions
    {
        public static Helpers.Includer GetIncluder(this List<Helpers.Includer> list, string path)
        {
            var splitted = path.Split('.');
            Helpers.Includer parent = list.Where(op => op.Name == splitted[0]).FirstOrDefault(); ;
            if (parent != null)
            {
                foreach (var item in splitted.Skip(0))
                {
                    var tmpParent = parent.SubIncluders.Where(op => path.StartsWith(op.Name)).FirstOrDefault();
                    if (tmpParent != null)
                        parent = tmpParent;
                    else
                        break;
                }
            }
            return parent;
        }
    }
}
