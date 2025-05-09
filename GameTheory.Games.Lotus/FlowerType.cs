﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a specific type of flower.
    /// </summary>
    /// <remarks>
    /// The integer value of the card corresponds to the number of petals in each flower.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Adding a None value here would necessitate extra logic in most places that deal with sets of flowers and would thus have a performance cost.")]
    public enum FlowerType : byte
    {
        /// <summary>
        /// A flower with three petals.
        /// </summary>
        Iris = 3,

        /// <summary>
        /// A flower with four petals.
        /// </summary>
        Primrose = 4,

        /// <summary>
        /// A flower with five petals.
        /// </summary>
        CherryBlossom = 5,

        /// <summary>
        /// A flower with six petals.
        /// </summary>
        Lily = 6,

        /// <summary>
        /// A flower with seven petals.
        /// </summary>
        Lotus = 7,
    }
}
