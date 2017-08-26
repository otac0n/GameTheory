// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using Games.TwentyFortyEight;
    using TwentyFortyEight;

    /// <summary>
    /// Implements a console renderer for the game of <see cref="GameState">TwentyFortyEight</see>.
    /// </summary>
    public class TwentyFortyEightConsoleRenderer : IConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            var gameState = (GameState)state;
            new Templates().RenderGameState(gameState, Console.Out);
        }
    }
}
