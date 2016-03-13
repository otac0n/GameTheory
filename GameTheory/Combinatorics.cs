// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Provides combinatorics functions.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// Creates an enumerable collection containing all combinations of the specified items that contain the specified count of items.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="items">The items to generate combinations of.</param>
        /// <param name="count">The count of items in each combination.</param>
        /// <returns>An enumerable collection of combinations.</returns>
        public static IEnumerable<ImmutableList<T>> Combinations<T>(IList<T> items, int count)
        {
            if (count <= 0)
            {
                yield break;
            }

            if (count > items.Count)
            {
                yield break;
            }

            var indexes = new int[count];
            var prev = 0;
            var depth = 1;

            while (depth >= 0)
            {
                if (depth >= count)
                {
                    yield return indexes.Select(i => items[i]).ToImmutableList();
                    prev = --depth;
                    continue;
                }

                indexes[depth] = indexes[prev] + 1;

                if (indexes[depth] >= items.Count)
                {
                    prev = --depth;
                }
                else
                {
                    prev = depth++;
                }
            }
        }
    }
}
