// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    /// <summary>
    /// Represents the current phase of the game.
    /// </summary>
    public enum Phase : byte
    {
        /// <summary>
        /// The game is in play.
        /// </summary>
        Play,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
