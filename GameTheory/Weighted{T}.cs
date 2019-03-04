// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    /// <summary>
    /// Provides a concrete implementation of the <see cref="IWeighted{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of value being weighted.</typeparam>
    public struct Weighted<T> : IWeighted<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Weighted{T}"/> struct.
        /// </summary>
        /// <param name="value">The value being weighted.</param>
        /// <param name="weight">The weight assigned.</param>
        public Weighted(T value, double weight)
        {
            this.Value = value;
            this.Weight = weight;
        }

        /// <inheritdoc />
        public T Value { get; }

        /// <inheritdoc />
        public double Weight { get; }
    }
}
