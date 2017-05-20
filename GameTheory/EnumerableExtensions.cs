// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        public static IEnumerable<IReadOnlyList<T>> Partition<T>(this IEnumerable<T> source, int count)
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
    }
}
