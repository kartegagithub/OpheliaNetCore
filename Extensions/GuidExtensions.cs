using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia
{
    public static class GuidExtensions
    {
        /// <summary>
        /// Guid değerinin null veya boş olup olmadığına bakar.
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Değer null ise veya boş ise 1 döndürür.</returns>
        public static bool IsNullOrEmpty(this Guid target)
        {
            return target == null || target.Equals(Guid.Empty);
        }
    }
}
