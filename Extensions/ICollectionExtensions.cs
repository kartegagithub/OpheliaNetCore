using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ophelia
{
    public static class ICollectionExtensions
    {
        public static void Associate<T>(this ICollection<T> destination, ICollection<T> source)
        {
            var removedItems = destination.ToList();
            foreach (var current in source)
            {
                if (removedItems.Contains(current))
                    removedItems.Remove(current);
                else
                    destination.Add(current);
            }
            removedItems.Each(item => destination.Remove(item));
        }

        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (var item in source)
                destination.Add(item);
        }
    }
}
