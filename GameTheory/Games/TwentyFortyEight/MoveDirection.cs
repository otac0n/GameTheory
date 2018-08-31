// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight
{
    /// <summary>
    /// Represents the possible move directions.
    /// </summary>
    public enum MoveDirection : byte
    {
        /// <summary>
        /// Shift tiles upwards.
        /// </summary>
        Up,

        /// <summary>
        /// Shift tiles to the right.
        /// </summary>
        Right,

        /// <summary>
        /// Shift tiles downwards.
        /// </summary>
        Down,

        /// <summary>
        /// Shift tiles to the left.
        /// </summary>
        Left,
    }
}
