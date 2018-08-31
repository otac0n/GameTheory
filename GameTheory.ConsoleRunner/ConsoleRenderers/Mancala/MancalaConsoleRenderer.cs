// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.Mancala
{
    using System;
    using Games.Mancala;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Mancala</see>.
    /// </summary>
    public class MancalaConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc/>
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => this.Show((GameState)state);

        private void Show(GameState state) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
