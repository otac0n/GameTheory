// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.TwentyFortyEight
{
    using System;
    using Games.TwentyFortyEight;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">TwentyFortyEight</see>.
    /// </summary>
    public class TwentyFortyEightConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => this.Show((GameState)state);

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is MoveDirection move)
            {
                Console.Write(Resources.ResourceManager.GetEnumString(move));
            }
            else
            {
                base.RenderToken(state, token);
            }
        }

        private void Show(GameState state) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
