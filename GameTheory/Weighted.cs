// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    /// <summary>
    /// Provides a concrete way to construct <see cref="IWeighted{T}"/> values.
    /// </summary>
    public static class Weighted
    {
        /// <summary>
        /// Creates a new instance of a class implementing the <see cref="IWeighted{T}"/> interface.
        /// </summary>
        /// <param name="value">The value being weighted.</param>
        /// <param name="weight">The weight assigned.</param>
        /// <returns>The new instance.</returns>
        public static IWeighted<T> Create<T>(T value, double weight)
        {
            return new WeightedImpl<T>(value, weight);
        }

        private class WeightedImpl<T> : IWeighted<T>
        {
            public WeightedImpl(T value, double weight)
            {
                this.Value = value;
                this.Weight = weight;
            }

            public T Value { get; }

            public double Weight { get; }
        }
    }
}
