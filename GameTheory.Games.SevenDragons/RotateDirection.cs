// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons
{
    /// <summary>
    /// Represents a rotation direction.
    /// </summary>
    public enum RotateDirection : byte
    {
        /// <summary>
        /// Represents a rotation with turn order.
        /// </summary>
        AlongTurnOrder,

        /// <summary>
        /// Represents a rotation against turn order.
        /// </summary>
        OppositeTurnOrder,
    }
}
