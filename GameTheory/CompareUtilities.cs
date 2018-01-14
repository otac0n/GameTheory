// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides utility methods for composing comparable objects.
    /// </summary>
    public static class CompareUtilities
    {
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
