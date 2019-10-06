// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.$game$.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">$game$</see>.
    /// </summary>
    public class $game$ConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc/>
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => this.Show((GameState)state);

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            base.RenderToken(state, token);
        }

        private void Show(GameState state) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
