// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using FiveTribes;
    using Games.FiveTribes;

    /// <summary>
    /// Implements a console renderer for the game of <see cref="GameState">FiveTribes</see>.
    /// </summary>
    public class FiveTribesConsoleRenderer : IConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            var gameState = (GameState)state;
            new Templates(gameState).RenderGameState(gameState, Console.Out);
        }
    }
}
