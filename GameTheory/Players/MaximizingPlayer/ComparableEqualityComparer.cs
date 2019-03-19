// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;

    public class ComparableEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <inheritdoc />
        public bool Equals(T x, T y)
        {
            IComparable<T> comparable;
            if (object.ReferenceEquals(null, x))
            {
                return object.ReferenceEquals(null, y);
            }
            else if ((comparable = x as IComparable<T>) != null)
            {
                return comparable.CompareTo(y) == 0;
            }
            else
            {
                return x.Equals(y);
            }
        }

        /// <inheritdoc />
        public int GetHashCode(T obj) => 0;
    }
}
