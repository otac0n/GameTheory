// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    /// <summary>
    /// The possible outcomes for a game.
    /// </summary>
    public enum Result : short
    {
        /// <summary>
        /// The outcome has not yet been determined.
        /// </summary>
        None = 0,

        /// <summary>
        /// The game will result in a loss for the player.
        /// </summary>
        /// <remarks>
        /// A loss occurs when the game has no subsequent moves, the player is not a winner, and some other player is a winner.
        /// </remarks>
        Loss = -2,

        /// <summary>
        /// The game will result in an impasse.
        /// </summary>
        /// <remarks>
        /// An impasse (or stalemate) occurs when the game has no subsequent moves and there are no winners.
        /// </remarks>
        Impasse = -1,

        /// <summary>
        /// The game will result in a draw.
        /// </summary>
        /// <remarks>
        /// A draw occurs when the game has no subsequent moves, the player is a winner, and some other player is also a winner.
        /// </remarks>
        SharedWin = 1,

        /// <summary>
        /// The game will result in a win.
        /// </summary>
        /// <remarks>
        /// A win occurs when the game has no subsequen moves, the player is a winner, and no other player is a winner.
        /// </remarks>
        Win = 2,
    }
}
