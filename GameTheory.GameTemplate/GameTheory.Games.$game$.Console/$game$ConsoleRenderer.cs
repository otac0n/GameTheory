// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.$game$.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">$game$</see>.
    /// </summary>
    public class $game$ConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc/>
        public override void Show(GameState state, PlayerToken playerToken = null) => new Templates(playerToken).RenderGameState((GameState)state, this.MakeRenderTokenWriter(state));

        /// <inheritdoc/>
        protected override void RenderToken(GameState state, object token)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            base.RenderToken(state, token);
        }
    }
}
