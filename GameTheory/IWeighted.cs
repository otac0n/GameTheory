// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    /// <summary>
    /// Represents a weighted value.
    /// </summary>
    /// <typeparam name="T">The type of value being weighted.</typeparam>
    public interface IWeighted<out T>
    {
        /// <summary>
        /// Gets the value being weighted.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Gets the weight assigned.
        /// </summary>
        double Weight { get; }
    }
}
