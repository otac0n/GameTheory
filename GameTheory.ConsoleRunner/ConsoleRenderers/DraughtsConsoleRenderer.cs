// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using Draughts;
    using Games.Draughts;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Draughts</see>.
    /// </summary>
    public class DraughtsConsoleRenderer : BaseConsoleRenderer<Move>
    {
        /// <inheritdoc/>
        public override void Show(IGameState<Move> state, PlayerToken playerToken = null)
        {
            var gameState = (GameState)state;
            new Templates().RenderGameState(gameState, this.MakeRenderTokenWriter(state));
        }

        /// <inheritdoc/>
        protected override void RenderToken(IGameState<Move> state, object token)
        {
            if (token is Piece piece)
            {
                PlayerToken player = null;
                var pieceStr = " ";
                switch (piece & (Piece.White | Piece.Black | Piece.Crowned))
                {
                    case Piece.White:
                        player = state.Players[0];
                        pieceStr = "o";
                        break;

                    case Piece.Black:
                        player = state.Players[1];
                        pieceStr = "o";
                        break;

                    case Piece.White | Piece.Crowned:
                        player = state.Players[0];
                        pieceStr = "8";
                        break;

                    case Piece.Black | Piece.Crowned:
                        player = state.Players[1];
                        pieceStr = "8";
                        break;

                    case Piece.None:
                        break;
                }

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
    }
}
