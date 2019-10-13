// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Provides combinatorics functions.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// Compute the binomial coefficient, or n-choose-k.
        /// </summary>
        /// <param name="n">The number of elements to choose from.</param>
        /// <param name="k">The number of elements chosen.</param>
        /// <returns>The binomial coefficient, or the number of combinations.</returns>
        public static BigInteger Choose(int n, int k) => Choose((BigInteger)n, (BigInteger)k);

        /// <summary>
        /// Compute the binomial coefficient, or n-choose-k.
        /// </summary>
        /// <param name="n">The number of elements to choose from.</param>
        /// <param name="k">The number of elements chosen.</param>
        /// <returns>The binomial coefficient, or the number of combinations.</returns>
        public static BigInteger Choose(BigInteger n, BigInteger k)
        {
            if (n < k)
            {
                return 0;
            }

            BigInteger result = 1;
            for (var i = 1; i <= k; i++)
            {
                result *= n - (k - i);
                result /= i;
            }

            return result;
        }

        /// <summary>
        /// Computes the number of variations.
        /// </summary>
        /// <param name="n">The number of elements to choose from.</param>
        /// <param name="k">The number of elements chosen.</param>
        /// <returns>The number of variations.</returns>
        public static BigInteger Permute(int n, int k) => Permute((BigInteger)n, (BigInteger)k);

        /// <summary>
        /// Computes the number of variations.
        /// </summary>
        /// <param name="n">The number of elements to choose from.</param>
        /// <param name="k">The number of elements chosen.</param>
        /// <returns>The number of variations.</returns>
        public static BigInteger Permute(BigInteger n, BigInteger k)
        {
            if (n < k)
            {
                return 0;
            }

            var min = n - k + 1;
            var max = n;

            BigInteger result = 1;
            for (var i = max; i >= min; i--)
            {
                result *= i;
            }

            return result;
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

            bool Increment(int i)
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
                    return Increment(i + 1);
                }

                return false;
            }

            while (!Increment(0))
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
        /// Gets an enumerable collection containing all combinations of the items in this collection.
        /// </summary>
        /// <param name="itemCounts">The count of each disctinct item available.</param>
        /// <param name="count">The size of the combinations.</param>
        /// <returns>An enumerable collection of weighted combinations.</returns>
        public static IEnumerable<Weighted<ImmutableList<int>>> WeightedCombinations(IList<int> itemCounts, int count)
        {
            foreach (var c in Combinations(itemCounts, count))
            {
                yield return Weighted.Create(c, (double)c.Aggregate((BigInteger)1, (p, x) => p * Choose(itemCounts[x], c[x])));
            }
        }
    }
}
