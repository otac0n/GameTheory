// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers.Chess
{
    using System;
    using Games.Chess;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Chess</see>.
    /// </summary>
    public class ChessConsoleRenderer : BaseConsoleRenderer<Move>
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

            if (token is Pieces piece)
            {
                PlayerToken player = null;
                var gameState = (GameState)state;

                switch (piece & PieceMasks.Colors)
                {
                    case Pieces.White:
                        player = state.Players[0];
                        break;

                    case Pieces.Black:
                        player = state.Players[1];
                        break;
                }

                var pieceStr = piece == Pieces.None
                    ? " "
                    : gameState.Variant.NotationSystem.Format(piece);

                if (player != null)
                {
                    ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor(state, player), () =>
                    {
                        Console.Write(pieceStr);
                    });
                }
                else
                {
                    Console.Write(pieceStr);
                }
            }
            else
            {
                base.RenderToken(state, token);
            }
        }

        private void Show(GameState state) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
