// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;

    /// <summary>
    /// Special powers available to players.
    /// </summary>
    [Flags]
    public enum SpecialPower
    {
        /// <summary>
        /// No special powers.
        /// </summary>
        None = 0,

        /// <summary>
        /// Grants an extra guardian.
        /// </summary>
        ElderGuardian = 0x01,

        /// <summary>
        /// Increases a players hand size by one.
        /// </summary>
        EnlightenedPath = 0x02,

        /// <summary>
        /// Removes the two-card limit when playing petal cards.
        /// </summary>
        InfiniteGrowth = 0x04,
    }
}
