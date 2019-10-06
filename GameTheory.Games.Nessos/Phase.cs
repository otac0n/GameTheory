// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    /// <summary>
    /// Represents the current phase of the game.
    /// </summary>
    public enum Phase
    {
        /// <summary>
        /// The game is in play.
        /// </summary>
        Play,

        /// <summary>
        /// The players must draw up to their hand limit before play can continue.
        /// </summary>
        Draw,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
