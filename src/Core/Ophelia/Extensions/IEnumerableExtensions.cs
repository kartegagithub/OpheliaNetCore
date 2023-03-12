using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia
{
    public static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection == null)
                return;

            foreach (T item in collection)
                action(item);
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(grps => grps).Select(e => e.First());
        }

        public static IEnumerable<IEnumerable<T>> Slice<T>(this IEnumerable<T> source, int size)
        {
            var sourceList = source.ToList();
            int current = 0;

            while (current < sourceList.Count)
            {
                yield return sourceList.GetRange(current, Math.Min(size, sourceList.Count - current));
                current += size;
            }
        }

        public static Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>> Pivot<TSource, TFirstKey, TSecondKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TFirstKey> firstKeySelector, Func<TSource, TSecondKey> secondKeySelector, Func<IEnumerable<TSource>, TValue> aggregate)
        {
            var returnValue = new Dictionary<TFirstKey, Dictionary<TSecondKey, TValue>>();

            var lookup = source.ToLookup(firstKeySelector);
            foreach (var item in lookup)
            {
                var dictionary = new Dictionary<TSecondKey, TValue>();
                returnValue.Add(item.Key, dictionary);
                var subDictionary = item.ToLookup(secondKeySelector);
                foreach (var subitem in subDictionary)
                {
                    dictionary.Add(subitem.Key, aggregate(subitem));
                }
            }

            return returnValue;
        }

        public static IEnumerable<T> Replace<T>(this IEnumerable<T> collection, T source, T replacement)
        {
            IEnumerable<T> collectionWithout = collection;
            if (source != null)
            {
                collectionWithout = collectionWithout.Except(new[] { source });
            }
            return collectionWithout.Union(new[] { replacement });
        }

        public static string Concatenate(this IEnumerable<string> values)
        {
            return values.Concatenate(string.Empty);
        }

        public static string Concatenate(this IEnumerable<string> values, string separator)
        {
            return values.Concatenate(separator, string.Empty, string.Empty);
        }

        public static string Concatenate(this IEnumerable<string> values, string separator, string prefix, string suffix)
        {
            if (!values.Any())
                return string.Empty;

            return values
                .Select(item => string.Concat(prefix, item, suffix))
                .Aggregate((accu, item) => accu += string.Concat(separator, item));
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            var collection = new ObservableCollection<T>();
            foreach (var item in source)
                collection.Add(item);
            return collection;
        }

        public static Dictionary<TKey, List<TValue>> ToDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groupings)
        {
            return groupings.ToDictionary(group => group.Key, group => group.ToList());
        }

        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePair<T>(this IEnumerable<T> items,
            Func<T, string> keySelector,
            Func<T, string> valueSelector,
            Func<T, bool> predicate)
        {
            if (predicate != null)
                items = items.Where(predicate);

            foreach (var item in items)
                yield return new KeyValuePair<string, string>(
                        key: keySelector(item),
                        value: valueSelector(item));
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            var result = source.FirstOrDefault();
            if (result == null)
                return value;
            return result;
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource value)
        {
            var result = source.FirstOrDefault(predicate);
            if (result == null)
                return value;
            return result;
        }

        public static IEnumerable<TResult> Paginate<TResult>(
           this IEnumerable<TResult> source, int page, int pageSize)
        {
            if (pageSize > 0 && page > 0)
            {
                int skip = Math.Max(pageSize * (page - 1), 0);
                return source.Skip(skip).Take(pageSize);
            }
            return source;
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> target)
        {
            Random randomizer = new Random();
            return target.OrderBy(x => (randomizer.Next()));
        }

        public static IEnumerable<T> TakeRandom<T>(
           this IEnumerable<T> source, int count)
        {
            var sourceArray = source.ToArray();
            var selectedItems = TakeRandomIntegersInternal(count, sourceArray.Length);
            foreach (var index in selectedItems)
                yield return sourceArray[index];
        }

        private static IEnumerable<int> TakeRandomIntegersInternal(int count, int length)
        {
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            HashSet<int> check = new HashSet<int>();
            var maxCount = count > length ? length : count;
            var maxValue = length + 1;
            var currentValue = rand.Next(1, maxValue);
            while (check.Count < maxCount)
            {
                if (!check.Contains(currentValue))
                {
                    check.Add(currentValue);
                    yield return currentValue - 1;
                }
                currentValue = rand.Next(1, maxValue);
            }
        }

        public static IEnumerable<TResult> TakeLast<TResult>(this IEnumerable<TResult> source, int count)
        {
            return source
                .Reverse()
                .Take(count)
                .Reverse(); ;
        }

        private static IEnumerable<TSource> ApplyOrder<TSource>(IEnumerable<TSource> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(TSource);
            ParameterExpression arg = Expression.Parameter(type, "p");
            Expression expr = arg;
            foreach (string prop in props)
            {
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(TSource), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Enumerable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(TSource), type)
                    .Invoke(null, new object[] { source, lambda.Compile() });

            return result as IEnumerable<TSource>;
        }

        public static IEnumerable<TSource> ThenBy<TSource>(this IEnumerable<TSource> source, string property)
        {
            string descParameter = " Desc";
            string propertyName = property;
            string methodName = "ThenBy";
            if (property.EndsWith(descParameter, StringComparison.InvariantCultureIgnoreCase))
            {
                propertyName = property.Replace(descParameter, string.Empty);
                methodName = "ThenByDescending";
            }
            return ApplyOrder<TSource>(source, propertyName, methodName);
        }

        public static IEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, string property)
        {
            string descParameter = " Desc";
            string propertyName = property;
            string methodName = "OrderBy";
            if (property.EndsWith(descParameter, StringComparison.InvariantCultureIgnoreCase))
            {
                propertyName = property.Replace(descParameter, string.Empty);
                methodName = "OrderByDescending";
            }
            return ApplyOrder<TSource>(source, propertyName, methodName);
        }

        public static bool AggregateOr<TSource>(this IEnumerable<TSource> source, bool seed, Func<bool, TSource, bool> func)
        {
            bool runningValue = seed;

            foreach (TSource element in source)
            {
                if (runningValue != seed) break;
                runningValue = func(runningValue, element);
            }

            return runningValue;
        }

        public static IEnumerable<TResult> LeftOuterJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector,
                (outerItem, innerGroup) => innerGroup.DefaultIfEmpty()
                    .Select(innerGroupItem => resultSelector(outerItem, innerGroupItem))
            ).SelectMany(result => result);
        }

        public static IEnumerable<TResult> RightOuterJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return inner.LeftOuterJoin(outer,
                innerKeySelector, outerKeySelector,
                (innerItem, outerItem) => resultSelector(outerItem, innerItem)
            );
        }

        public static IEnumerable<TResult> FullOuterJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.LeftOuterJoin(inner, outerKeySelector, innerKeySelector, resultSelector).Concat(
                inner.Where(innerItem =>
                    !outer.Select(outerItem => outerKeySelector(outerItem)).Contains(innerKeySelector(innerItem))
                ).Select(innerItem => resultSelector(default(TOuter), innerItem))
            );
        }

        public static IEnumerable<T> Cache<T>(this IEnumerable<T> source)
        {
            return CacheHelper(source.GetEnumerator());
        }

        public static IEnumerable<TResult> Where<TResult>(this IEnumerable<TResult> source, string property, object value) where TResult : class
        {
            return source.Where(item => item != null && item.GetPropertyValue(property).Equals(value));
        }

        #region "Helpers"
        private static IEnumerable<T> CacheHelper<T>(IEnumerator<T> source)
        {
            var isEmpty = new Lazy<bool>(() => !source.MoveNext());
            var head = new Lazy<T>(() => source.Current);
            var tail = new Lazy<IEnumerable<T>>(() => CacheHelper(source));

            return CacheHelper(isEmpty, head, tail);
        }

        private static IEnumerable<T> CacheHelper<T>(
            Lazy<bool> isEmpty,
            Lazy<T> head,
            Lazy<IEnumerable<T>> tail)
        {
            if (isEmpty.Value)
                yield break;

            yield return head.Value;
            foreach (var value in tail.Value)
                yield return value;
        }
        #endregion
    }
}
