// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides basic rendering for more complex console renderers.
    /// </summary>
    /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
    public abstract class BaseConsoleRenderer<TMove> : IConsoleRenderer<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public abstract void Show(IGameState<TMove> state, PlayerToken playerToken = null);

        /// <inheritdoc/>
        public void Show(IGameState<TMove> state, IList<object> formatTokens)
        {
            foreach (var token in FormatUtilities.FlattenFormatTokens(formatTokens))
            {
                this.RenderToken(state, token);
            }
        }

        /// <summary>
        /// Renders an atomic token.
        /// </summary>
        /// <param name="state">The context game state.</param>
        /// <param name="token">The token to be rendered.</param>
        protected virtual void RenderToken(IGameState<TMove> state, object token)
        {
            if (token is PlayerToken playerToken)
            {
                ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor(state, playerToken), () =>
                {
                    Console.Write(state.GetPlayerName(playerToken));
                });
            }
            else
            {
                Console.Write(token);
            }
        }

        /// <inheritdoc/>
        public void Show(IGameState<TMove> state, ITokenFormattable tokenFormattable)
        {
            this.Show(state, tokenFormattable.FormatTokens);
        }
    }
}
