// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Provides utility methods for composing comparable objects.
    /// </summary>
    public static class CompareUtilities
    {
        /// <summary>
        /// Compares two dictionaries of comparable keys and elements.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in each dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in each dictionary.</typeparam>
        /// <param name="left">The first dictionary.</param>
        /// <param name="right">The second dictionary.</param>
        /// <param name="keyComparer">An optional comparer to use when evaluating keys.</param>
        /// <param name="valueComparer">An optional comparer to use when evaluating values.</param>
        /// <returns>A value indicating whether the dictionaries are the same size and contain the same keys and elements.</returns>
        public static int CompareDictionaries<TKey, TValue>(ImmutableDictionary<TKey, TValue> left, ImmutableDictionary<TKey, TValue> right, IComparer<TKey> keyComparer = null, IComparer<TValue> valueComparer = null)
            where TKey : IComparable<TKey>
            where TValue : IComparable<TValue>
        {
            if (left == right)
            {
                return 0;
            }

            int comp;

            if ((comp = left.Count.CompareTo(right.Count)) != 0)
            {
                return comp;
            }

            keyComparer = keyComparer ?? Comparer<TKey>.Default;
            valueComparer = valueComparer ?? Comparer<TValue>.Default;

            var otherKeys = new List<TKey>(right.Count);
            otherKeys.AddRange(right.Keys);
            otherKeys.Sort(keyComparer);

            var i = 0;
            foreach (var a in left.Keys.OrderBy(k => k, keyComparer))
            {
                var b = otherKeys[i++];

                if ((comp = keyComparer.Compare(a, b)) != 0 ||
                    (comp = valueComparer.Compare(left[a], right[b])) != 0)
                {
                    return comp;
                }
            }

            return 0;
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in each list.</typeparam>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareEnumLists<T>(IList<T> left, IList<T> right)
            where T : IComparable
        {
            var comp = 0;

            if (!object.ReferenceEquals(left, right))
            {
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
            }

            return comp;
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in each list.</typeparam>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareLists<T>(IList<T> left, IList<T> right)
            where T : class, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(left, right))
            {
                if ((comp = left.Count.CompareTo(right.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < left.Count; i++)
                {
                    var l = left[i];
                    var r = right[i];
                    if (!object.ReferenceEquals(l, r))
                    {
                        if (l == null)
                        {
                            return -1;
                        }

                        if ((comp = l.CompareTo(r)) != 0)
                        {
                            return comp;
                        }
                    }
                }
            }

            return comp;
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in each list.</typeparam>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareReadOnlyEnumLists<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
            where T : IComparable
        {
            var comp = 0;

            if (!object.ReferenceEquals(left, right))
            {
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
            }

            return comp;
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in each list.</typeparam>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareReadOnlyLists<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
            where T : class, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(left, right))
            {
                if ((comp = left.Count.CompareTo(right.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < left.Count; i++)
                {
                    var l = left[i];
                    var r = right[i];
                    if (!object.ReferenceEquals(l, r))
                    {
                        if (l == null)
                        {
                            return -1;
                        }

                        if ((comp = l.CompareTo(r)) != 0)
                        {
                            return comp;
                        }
                    }
                }
            }

            return comp;
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in each list.</typeparam>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareReadOnlyValueLists<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
            where T : struct, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(left, right))
            {
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
            }

            return comp;
        }

        /// <summary>
        /// Compares two nullable elements.
        /// </summary>
        /// <typeparam name="T">The underlying type of nullable elements to compare.</typeparam>
        /// <param name="left">The first item.</param>
        /// <param name="right">The second item.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public static int CompareTo<T>(this T? left, T? right)
            where T : struct, IComparable<T>
        {
            if (left == null)
            {
                return right == null ? 0 : -1;
            }
            else if (right == null)
            {
                return 1;
            }

            return left.Value.CompareTo(right.Value);
        }

        /// <summary>
        /// Compares two <see cref="Maybe{T}"/> values.
        /// </summary>
        /// <typeparam name="T">The type of value elements to compare.</typeparam>
        /// <param name="left">The first item.</param>
        /// <param name="right">The second item.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public static int CompareTo<T>(this Maybe<T> left, Maybe<T> right)
            where T : struct, IComparable<T>
        {
            if (!left.HasValue)
            {
                return !right.HasValue ? 0 : -1;
            }
            else if (!right.HasValue)
            {
                return 1;
            }

            return left.Value.CompareTo(right.Value);
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in each list.</typeparam>
        /// <param name="left">The first list.</param>
        /// <param name="right">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareValueLists<T>(IList<T> left, IList<T> right)
            where T : struct, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(left, right))
            {
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
            }

            return comp;
        }
    }
}
