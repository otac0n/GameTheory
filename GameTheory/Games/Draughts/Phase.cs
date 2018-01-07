// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    /// <summary>
    /// The current phase of the game.
    /// </summary>
    public enum Phase
    {
        /// <summary>
        /// The game is in play.
        /// </summary>
        Play,

        /// <summary>
        /// The player is removing a piece.
        /// </summary>
        RemovePiece,

        /// <summary>
        /// End of the game.
        /// </summary>
        End,
    }
}
