// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Console
{
    using System;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Draughts</see>.
    /// </summary>
    public class DraughtsConsoleRenderer : BaseConsoleRenderer<Move>
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
                var pieceStr = " ";
                switch (piece & (Pieces.White | Pieces.Black | Pieces.Crowned))
                {
                    case Pieces.White:
                        player = state.Players[0];
                        pieceStr = "o";
                        break;

                    case Pieces.Black:
                        player = state.Players[1];
                        pieceStr = "o";
                        break;

                    case Pieces.White | Pieces.Crowned:
                        player = state.Players[0];
                        pieceStr = "8";
                        break;

                    case Pieces.Black | Pieces.Crowned:
                        player = state.Players[1];
                        pieceStr = "8";
                        break;

                    case Pieces.None:
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

        private void Show(GameState state) => new Templates().RenderGameState(state, this.MakeRenderTokenWriter(state));
    }
}
