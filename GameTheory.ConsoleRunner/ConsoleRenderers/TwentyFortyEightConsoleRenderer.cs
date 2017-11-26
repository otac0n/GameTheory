// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using Games.TwentyFortyEight;
    using TwentyFortyEight;

    /// <summary>
    /// Implements a console renderer for the game of <see cref="GameState">TwentyFortyEight</see>.
    /// </summary>
    public class TwentyFortyEightConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            var gameState = (GameState)state;
            new Templates().RenderGameState(gameState, this.MakeRenderTokenWriter(state));
        }
    }
}
