﻿using System;

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
            return target.Equals(Guid.Empty);
        }
    }
}
