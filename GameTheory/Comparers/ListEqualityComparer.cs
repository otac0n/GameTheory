// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Comparers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Compares lists of elements for equality.
    /// </summary>
    /// <typeparam name="T">The type of elements in each list.</typeparam>
    public class ListEqualityComparer<T> : IEqualityComparer<IList<T>>, IEqualityComparer<ImmutableArray<T>>
        where T : class
    {
        private readonly IEqualityComparer<T> innerComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListEqualityComparer{T}"/> class.
        /// </summary>
        public ListEqualityComparer()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListEqualityComparer{T}"/> class.
        /// </summary>
        /// <param name="equalityComparer">The inner equality comparer to use.</param>
        public ListEqualityComparer(IEqualityComparer<T> equalityComparer)
        {
            this.innerComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }

        /// <inheritdoc/>
        public bool Equals(IList<T> x, IList<T> y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (object.ReferenceEquals(x, null) ||
                object.ReferenceEquals(y, null) ||
                x.Count != y.Count)
            {
                return false;
            }

            for (var i = 0; i < x.Count; i++)
            {
                if (!this.innerComparer.Equals(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (this.innerComparer.Equals(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        int IEqualityComparer<IList<T>>.GetHashCode(IList<T> list)
        {
            var hash = HashUtilities.Seed;

            for (var i = 0; i < list.Count; i++)
            {
                HashUtilities.Combine(ref hash, list[i] is T t ? this.innerComparer.GetHashCode(t) : i);
            }

            return hash;
        }

        /// <inheritdoc/>
        int IEqualityComparer<ImmutableArray<T>>.GetHashCode(ImmutableArray<T> list)
        {
            var hash = HashUtilities.Seed;

            for (var i = 0; i < list.Length; i++)
            {
                HashUtilities.Combine(ref hash, list[i] is T t ? this.innerComparer.GetHashCode(t) : i);
            }

            return hash;
        }
    }
}
