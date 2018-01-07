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
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareEnumLists<T>(IList<T> a, IList<T> b)
            where T : IComparable
        {
            var comp = 0;

            if (!object.ReferenceEquals(a, b))
            {
                if ((comp = a.Count.CompareTo(b.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < a.Count; i++)
                {
                    if ((comp = a[i].CompareTo(b[i])) != 0)
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
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareLists<T>(IList<T> a, IList<T> b)
            where T : class, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(a, b))
            {
                if ((comp = a.Count.CompareTo(b.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < a.Count; i++)
                {
                    var left = a[i];
                    var right = b[i];
                    if (!object.ReferenceEquals(left, right))
                    {
                        if (left == null)
                        {
                            return -1;
                        }

                        if ((comp = left.CompareTo(right)) != 0)
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
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareReadOnlyEnumLists<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
            where T : IComparable
        {
            var comp = 0;

            if (!object.ReferenceEquals(a, b))
            {
                if ((comp = a.Count.CompareTo(b.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < a.Count; i++)
                {
                    if ((comp = a[i].CompareTo(b[i])) != 0)
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
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareReadOnlyLists<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
            where T : class, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(a, b))
            {
                if ((comp = a.Count.CompareTo(b.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < a.Count; i++)
                {
                    var left = a[i];
                    var right = b[i];
                    if (!object.ReferenceEquals(left, right))
                    {
                        if (left == null)
                        {
                            return -1;
                        }

                        if ((comp = left.CompareTo(right)) != 0)
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
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareReadOnlyValueLists<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
            where T : struct, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(a, b))
            {
                if ((comp = a.Count.CompareTo(b.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < a.Count; i++)
                {
                    if ((comp = a[i].CompareTo(b[i])) != 0)
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
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public static int CompareTo<T>(this T? a, T? b)
            where T : struct, IComparable<T>
        {
            if (a == null)
            {
                return b == null ? 0 : -1;
            }
            else if (b == null)
            {
                return 1;
            }

            return a.Value.CompareTo(b.Value);
        }

        /// <summary>
        /// Compares two <see cref="Maybe{T}"/> values.
        /// </summary>
        /// <typeparam name="T">The type of value elements to compare.</typeparam>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public static int CompareTo<T>(this Maybe<T> a, Maybe<T> b)
            where T : struct, IComparable<T>
        {
            if (!a.HasValue)
            {
                return !b.HasValue ? 0 : -1;
            }
            else if (!b.HasValue)
            {
                return 1;
            }

            return a.Value.CompareTo(b.Value);
        }

        /// <summary>
        /// Compares two lists of comparable elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in each list.</typeparam>
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        /// <returns>A value indicating whether the lists are the same length and contain the same elements.</returns>
        public static int CompareValueLists<T>(IList<T> a, IList<T> b)
            where T : struct, IComparable<T>
        {
            var comp = 0;

            if (!object.ReferenceEquals(a, b))
            {
                if ((comp = a.Count.CompareTo(b.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < a.Count; i++)
                {
                    if ((comp = a[i].CompareTo(b[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            return comp;
        }
    }
}
