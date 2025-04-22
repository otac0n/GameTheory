// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Console
{
    using System;
    using System.Linq;
    using GameTheory.ConsoleRunner.Shared;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Chess</see>.
    /// </summary>
    [ConsoleFont("Consolas", 8, 18)]
    public class ChessConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc/>
        public override void Show(GameState state, PlayerToken playerToken = null)
        {
            var ranks = Enumerable.Range(0, state.Variant.Height);
            var files = Enumerable.Range(0, state.Variant.Width);
            if (playerToken == null || state.Players.IndexOf(playerToken) <= 0)
            {
                ranks = ranks.Reverse();
            }
            else
            {
                files = files.Reverse();
            }

            foreach (var y in ranks)
            {
                foreach (var x in files)
                {
                    ConsoleInteraction.WithColor(Console.ForegroundColor, (x + y) % 2 == 0 ? ConsoleColor.Black : ConsoleColor.Gray, () =>
                    {
                        Console.Write(' ');
                        this.RenderToken(state, state.GetPieceAt(x, y));
                        Console.Write(' ');
                    });
                }

                Console.WriteLine($" {y + 1}");
            }

            foreach (var x in files)
            {
                Console.Write($" {(char)('a' + x)} ");
            }

            Console.WriteLine();
        }

        /// <inheritdoc/>
        protected override void RenderToken(GameState state, object token)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (token is Pieces piece)
            {
                PlayerToken player = null;

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
                    : state.Variant.NotationSystem.Format(piece);

                if (player != null)
                {
                    ConsoleInteraction.WithColor(ConsoleInteraction.GetPlayerColor<GameState, Move>(state, player), () =>
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
