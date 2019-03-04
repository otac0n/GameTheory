// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        /// <typeparam name="T">The type of value being weighted.</typeparam>
        /// <param name="value">The value being weighted.</param>
        /// <param name="weight">The weight assigned.</param>
        /// <returns>The new instance.</returns>
        public static Weighted<T> Create<T>(T value, double weight)
        {
            return new Weighted<T>(value, weight);
        }
    }
}
