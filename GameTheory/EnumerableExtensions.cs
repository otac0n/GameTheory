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
