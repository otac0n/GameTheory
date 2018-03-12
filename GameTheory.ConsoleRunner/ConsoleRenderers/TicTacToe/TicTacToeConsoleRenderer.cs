// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.TicTacToe
{
    using System;
    using Games.TicTacToe;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">TicTacToe</see>.
    /// </summary>
    public class TicTacToeConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc/>
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null) => this.Show((GameState)state, playerToken);

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is PlayerToken playerToken)
            {
                ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor(state, playerToken), () =>
                {
                    Console.Write(playerToken == state.Players[0] ? "X" : "O");
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }

        private void Show(GameState state, PlayerToken playerToken)
        {
            new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
        }
    }
}
