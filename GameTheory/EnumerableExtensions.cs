// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Provides additional functions on enumerable collections.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Gets the collection of elements that match the maximum according to the specified comparer.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The collection of elements that match the maximum.</returns>
        public static List<T> AllMax<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            var max = default(T);
            var list = new List<T>();
            foreach (var item in source)
            {
                var comp = list.Count == 0 ? 1 : comparer.Compare(item, max);
                if (comp == 0)
                {
                    list.Add(item);
                }
                else if (comp > 0)
                {
                    max = item;
                    list.Clear();
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Gets the collection of elements that match the maximum according to the specified element selector and comparer.
        /// </summary>
        /// <typeparam name="TItem">The type of items in the collection.</typeparam>
        /// <typeparam name="TElement">The type of element used to compare items.</typeparam>
        /// <param name="source">The list of items to compare.</param>
        /// <param name="elementSelector">Gets the element to compare for each item in the collection.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The collection of elements that match the maximum.</returns>
        public static List<TItem> AllMaxBy<TItem, TElement>(this IEnumerable<TItem> source, Func<TItem, TElement> elementSelector, IComparer<TElement> comparer = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            comparer = comparer ?? Comparer<TElement>.Default;

            var max = default(TElement);
            var list = new List<TItem>();
            foreach (var item in source)
            {
                var element = elementSelector(item);
                var comp = list.Count == 0 ? 1 : comparer.Compare(element, max);
                if (comp == 0)
                {
                    list.Add(item);
                }
                else if (comp > 0)
                {
                    max = element;
                    list.Clear();
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Gets the collection of elements that match the minimum according to the specified comparer.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The collection of elements that match the minimum.</returns>
        public static List<T> AllMin<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            var min = default(T);
            var list = new List<T>();
            foreach (var item in source)
            {
                var comp = list.Count == 0 ? -1 : comparer.Compare(item, min);
                if (comp == 0)
                {
                    list.Add(item);
                }
                else if (comp < 0)
                {
                    min = item;
                    list.Clear();
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Gets the collection of elements that match the minimum according to the specified element selector and comparer.
        /// </summary>
        /// <typeparam name="TItem">The type of items in the collection.</typeparam>
        /// <typeparam name="TElement">The type of element used to compare items.</typeparam>
        /// <param name="source">The list of items to compare.</param>
        /// <param name="elementSelector">Gets the element to compare for each item in the collection.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The collection of elements that match the minimum.</returns>
        public static List<TItem> AllMinBy<TItem, TElement>(this IEnumerable<TItem> source, Func<TItem, TElement> elementSelector, IComparer<TElement> comparer = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            comparer = comparer ?? Comparer<TElement>.Default;

            var min = default(TElement);
            var list = new List<TItem>();
            foreach (var item in source)
            {
                var element = elementSelector(item);
                var comp = list.Count == 0 ? -1 : comparer.Compare(element, min);
                if (comp == 0)
                {
                    list.Add(item);
                }
                else if (comp < 0)
                {
                    min = element;
                    list.Clear();
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Gets an enumerable collection containing the combinations of counts of unique items.
        /// </summary>
        /// <returns>An enumerable collection of combinations.</returns>
        /// <param name="itemCounts">The count of each disctinct item available.</param>
        /// <param name="count">The size of the combinations.</param>
        /// <param name="includeSmaller">A value indicating whether or not smaller combinations should also be returned.</param>
        /// <returns>A sequence that contains ways of choosing elements from the specified list.</returns>
        public static IEnumerable<ImmutableList<int>> Combinations(IList<int> itemCounts, int count, bool includeSmaller = false)
        {
            if (count <= 0)
            {
                yield break;
            }

            if (!includeSmaller && count > itemCounts.Sum())
            {
                yield break;
            }

            var storage = new int[itemCounts.Count];
            var digitalSum = 0;

            bool increment(int i)
            {
                if (i >= itemCounts.Count)
                {
                    return true;
                }

                var digit = ++storage[i];
                digitalSum++;

                if (digitalSum > count || digit > Math.Min(count, itemCounts[i]))
                {
                    storage[i] = 0;
                    digitalSum -= digit;
                    return increment(i + 1);
                }

                return false;
            }

            while (!increment(0))
            {
                if (includeSmaller || digitalSum == count)
                {
                    yield return storage.ToImmutableList();
                }
            }
        }

        /// <summary>
        /// Gets an enumerable collection containing all combinations of the items in the specified list.
        /// </summary>
        /// <returns>An enumerable collection of combinations.</returns>
        /// <typeparam name="T">The type of the elements of the input sequence.</typeparam>
        /// <param name="source">A list whose elements will chosen.</param>
        /// <param name="count">The size of the combinations.</param>
        /// <param name="includeSmaller">A value indicating whether or not smaller combinations should also be returned.</param>
        /// <returns>A sequence that contains ways of choosing elements from the specified list.</returns>
        public static IEnumerable<ImmutableList<T>> Combinations<T>(this IList<T> source, int count, bool includeSmaller = false)
        {
            foreach (var combinations in Combinations(source.Select(i => 1).ToArray(), count, includeSmaller))
            {
                yield return combinations.SelectMany((c, i) => Enumerable.Range(0, c).Select(_ => source[i])).ToImmutableList();
            }
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the default equality comparer to compare values.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the input sequences.</typeparam>
        /// <param name="source">An enumerable collection whose elements that are not also in second will be returned.</param>
        /// <param name="second">An array whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
        /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, params T[] second)
        {
            return source.Except(second.AsEnumerable());
        }

        /// <summary>
        /// Implements IndexOf for the <see cref="IReadOnlyList{T}"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The item to search for in the list.</param>
        /// <returns>The index of the item, if it is found; <c>-1</c>, otherwise.</returns>
        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];
                if (element == null)
                {
                    if (item == null)
                    {
                        return i;
                    }
                }
                else if (element.Equals(item))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the default equality comparer to compare values.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the input sequences.</typeparam>
        /// <param name="source">An enumerable collection whose distinct elements that also appear in second will be returned.</param>
        /// <param name="second">An array whose distinct elements that also appear in the first sequence will be returned.</param>
        /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
        public static IEnumerable<T> Intersect<T>(this IEnumerable<T> source, params T[] second)
        {
            return source.Intersect(second.AsEnumerable());
        }

        /// <summary>
        /// Gets the maximum element in a collection according to the specified comparer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The maxumum element.</returns>
        public static T Max<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            var any = false;
            var max = default(T);
            foreach (var item in source)
            {
                if (!any || comparer.Compare(item, max) > 0)
                {
                    max = item;
                    any = true;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the maximum element in a collection according to the specified comparison.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="comparison">The comparison to use.</param>
        /// <returns>The maxumum element.</returns>
        public static T Max<T>(this IEnumerable<T> source, Comparison<T> comparison)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparison == null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            var any = false;
            var max = default(T);
            foreach (var item in source)
            {
                if (!any || comparison(item, max) > 0)
                {
                    max = item;
                    any = true;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the maximum element in a collection according to the specified comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The maxumum element.</returns>
        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, IComparer<TResult> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            var any = false;
            var max = default(TResult);
            foreach (var item in source)
            {
                var selected = selector(item);
                if (!any || comparer.Compare(selected, max) > 0)
                {
                    max = selected;
                    any = true;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the maximum element in a collection according to the specified comparison.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="comparison">The comparison to use.</param>
        /// <returns>The maxumum element.</returns>
        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, Comparison<TResult> comparison)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparison == null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            var any = false;
            var max = default(TResult);
            foreach (var item in source)
            {
                var selected = selector(item);
                if (!any || comparison(selected, max) > 0)
                {
                    max = selected;
                    any = true;
                }
            }

            return max;
        }

        /// <summary>
        /// Gets the minimum element in a collection according to the specified comparer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The minimum element.</returns>
        public static T Min<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            var any = false;
            var min = default(T);
            foreach (var item in source)
            {
                if (!any || comparer.Compare(item, min) < 0)
                {
                    min = item;
                    any = true;
                }
            }

            return min;
        }

        /// <summary>
        /// Gets the minimum element in a collection according to the specified comparison.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="comparison">The comparison to use.</param>
        /// <returns>The minimum element.</returns>
        public static T Min<T>(this IEnumerable<T> source, Comparison<T> comparison)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparison == null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            var any = false;
            var min = default(T);
            foreach (var item in source)
            {
                if (!any || comparison(item, min) < 0)
                {
                    min = item;
                    any = true;
                }
            }

            return min;
        }

        /// <summary>
        /// Gets the minimum element in a collection according to the specified comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The minimum element.</returns>
        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, IComparer<TResult> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            var any = false;
            var min = default(TResult);
            foreach (var item in source)
            {
                var selected = selector(item);
                if (!any || comparer.Compare(selected, min) < 0)
                {
                    min = selected;
                    any = true;
                }
            }

            return min;
        }

        /// <summary>
        /// Gets the minimum element in a collection according to the specified comparison.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the collection.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">The list of elements to compare.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="comparison">The comparison to use.</param>
        /// <returns>The minimum element.</returns>
        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, Comparison<TResult> comparison)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparison == null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            var any = false;
            var min = default(TResult);
            foreach (var item in source)
            {
                var selected = selector(item);
                if (!any || comparison(selected, min) < 0)
                {
                    min = selected;
                    any = true;
                }
            }

            return min;
        }

        /// <summary>
        /// Partitions a list into many smaller chunks.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the input sequence.</typeparam>
        /// <param name="source">An enumerable collection whose elements will partitioned.</param>
        /// <param name="count">The size of each partition.</param>
        /// <returns>A sequence that contains partitions of the specified size.</returns>
        public static IEnumerable<ImmutableList<T>> Partition<T>(this IEnumerable<T> source, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var result = new List<T>(count);
            foreach (var item in source)
            {
                result.Add(item);
                if (result.Count >= count)
                {
                    yield return result.ToImmutableList();
                    result.Clear();
                }
            }

            if (result.Count > 0)
            {
                yield return result.ToImmutableList();
            }
        }

        /// <summary>
        /// Repeats a value.
        /// </summary>
        /// <typeparam name="T">The type of the value to repeat.</typeparam>
        /// <param name="value">The value to repeat.</param>
        /// <param name="count">The number of times to repeat the value.</param>
        /// <returns>A sequence containing the value repeated the specified number of times.</returns>
        public static IEnumerable<T> Times<T>(this T value, int count)
        {
            while (count-- > 0)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Initializes a <see cref="HashSet{T}"/> from the specified source.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
        /// <param name="source">The source of the elements in the set.</param>
        /// <param name="comparer">An option comparer to use.</param>
        /// <returns>The newly created set.</returns>
        public static HashSet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return comparer == null ? new HashSet<T>(source) : new HashSet<T>(source, comparer);
        }
    }
}
