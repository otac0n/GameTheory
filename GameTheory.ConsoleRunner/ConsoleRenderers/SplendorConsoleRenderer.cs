// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using Games.Splendor;
    using Splendor;

    /// <summary>
    /// Implements a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class SplendorConsoleRenderer : IConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            new Templates(playerToken).RenderGameState((GameState)state, Console.Out);
        }
    }
}
