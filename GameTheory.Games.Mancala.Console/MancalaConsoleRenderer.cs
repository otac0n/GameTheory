// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala.Console
{
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Mancala</see>.
    /// </summary>
    public class MancalaConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc/>
        public override void Show(GameState state, PlayerToken playerToken = null) => this.Show(state);

        private void Show(GameState state) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
