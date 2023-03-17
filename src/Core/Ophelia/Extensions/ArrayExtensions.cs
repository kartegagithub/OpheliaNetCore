using System;
using System.Collections.Generic;

namespace Ophelia
{
    public static class ArrayExtensions
    {
        public static void InsertItems<T>(this T[] array, IEnumerable<T> items)
        {
            int index = 0;
            foreach (var item in items)
            {
                if (index >= array.Length)
                    break;

                array[index] = item;
                index++;
            }
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static List<T> ToList<T>(this Array items, Func<object, T> mapFunction)
        {
            if (items == null || mapFunction == null)
                return new List<T>();

            List<T> list = new List<T>();
            for (int i = 0; i < items.Length; i++)
            {
                T val = mapFunction(items.GetValue(i));
                if (val != null)
                    list.Add(val);
            }
            return list;
        }

        public static List<T> ToList<T>(this object[] items)
        {
            return items.ToList(o => { return (T)o; });
        }

        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
