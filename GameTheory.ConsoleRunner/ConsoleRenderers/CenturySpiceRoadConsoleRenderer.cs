// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using CenturySpiceRoad;
    using Games.CenturySpiceRoad;

    /// <summary>
    /// Implements a console renderer for the game of <see cref="GameState">CenturySpiceRoad</see>.
    /// </summary>
    public class CenturySpiceRoadConsoleRenderer : IConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            new Templates(playerToken).RenderGameState((GameState)state, Console.Out);
        }
    }
}
