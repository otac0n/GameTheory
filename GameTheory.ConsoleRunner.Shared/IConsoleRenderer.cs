// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes the interface for a console renderer.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states that will be displayed.</typeparam>
    /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
    public interface IConsoleRenderer<TGameState, TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Shows the specified game state in the context of the specified player.
        /// </summary>
        /// <param name="state">The game state to render.</param>
        /// <param name="playerToken">The player whose view is being rendered.</param>
        void Show(TGameState state, PlayerToken playerToken = null);

        /// <summary>
        /// Shows the specified format tokens.
        /// </summary>
        /// <param name="state">The game to use when rendering.</param>
        /// <param name="formatTokens">The format tokens to render.</param>
        void Show(TGameState state, IList<object> formatTokens);

        /// <summary>
        /// Renders the specified token formattable object.
        /// </summary>
        /// <param name="state">The game to use when rendering.</param>
        /// <param name="tokenFormattable">The token formattable object to render.</param>
        void Show(TGameState state, ITokenFormattable tokenFormattable);
    }
}
