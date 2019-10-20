// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Comparers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Compares lists of comparable elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in each list.</typeparam>
    public class ValueListComparer<T> : IComparer<IList<T>>, IEqualityComparer<IList<T>>, IComparer<ImmutableArray<T>>, IEqualityComparer<ImmutableArray<T>>
        where T : struct, IComparable<T>
    {
        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int Compare(ImmutableArray<T> left, ImmutableArray<T> right)
        {
            int comp;
            if ((comp = left.Length.CompareTo(right.Length)) != 0)
            {
                return comp;
            }

            for (var i = 0; i < left.Length; i++)
            {
                if ((comp = left[i].CompareTo(right[i])) != 0)
                {
                    return comp;
                }
            }

            return comp;
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int Compare(IList<T> left, IList<T> right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }

            if (object.ReferenceEquals(left, null))
            {
                return -1;
            }

            if (object.ReferenceEquals(right, null))
            {
                return 1;
            }

            int comp;
            if ((comp = left.Count.CompareTo(right.Count)) != 0)
            {
                return comp;
            }

            for (var i = 0; i < left.Count; i++)
            {
                if ((comp = left[i].CompareTo(right[i])) != 0)
                {
                    return comp;
                }
            }

            return comp;
        }

        /// <summary>
        /// Provides a hash for the list.
        /// </summary>
        /// <param name="list">The list to hash.</param>
        /// <returns>A hash that matches when elements match.</returns>
        public static int GetHashCode(IList<T> list)
        {
            var hash = HashUtilities.Seed;

            for (var i = 0; i < list.Count; i++)
            {
                HashUtilities.Combine(ref hash, list[i].GetHashCode());
            }

            return hash;
        }

        /// <summary>
        /// Provides a hash for the list.
        /// </summary>
        /// <param name="list">The list to hash.</param>
        /// <returns>A hash that matches when elements match.</returns>
        public static int GetHashCode(ImmutableArray<T> list)
        {
            var hash = HashUtilities.Seed;

            for (var i = 0; i < list.Length; i++)
            {
                HashUtilities.Combine(ref hash, list[i].GetHashCode());
            }

            return hash;
        }

        /// <inheritdoc/>
        int IComparer<IList<T>>.Compare(IList<T> left, IList<T> right) =>
            ValueListComparer<T>.Compare(left, right);

        /// <inheritdoc/>
        int IComparer<ImmutableArray<T>>.Compare(ImmutableArray<T> left, ImmutableArray<T> right) =>
            ValueListComparer<T>.Compare(left, right);

        /// <inheritdoc/>
        public bool Equals(IList<T> x, IList<T> y) => Compare(x, y) == 0;

        /// <inheritdoc/>
        public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y) => Compare(x, y) == 0;

        /// <inheritdoc/>
        int IEqualityComparer<IList<T>>.GetHashCode(IList<T> list) =>
            ValueListComparer<T>.GetHashCode(list);

        /// <inheritdoc/>
        int IEqualityComparer<ImmutableArray<T>>.GetHashCode(ImmutableArray<T> list) =>
            ValueListComparer<T>.GetHashCode(list);
    }
}
