// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using GameTheory.Games.Chess;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Chess</see>.
    /// </summary>
    public class ChessMaximizingPlayer : MaximizingPlayer<Move, ResultScore<double>>
    {
        private static readonly ResultScoringMetric<Move, double> Metric =
            new ResultScoringMetric<Move, double>(ScoringMetric.Create<Move>(Score));

        /// <summary>
        /// Initializes a new instance of the <see cref="ChessMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public ChessMaximizingPlayer(PlayerToken playerToken, int minPly = 2)
            : base(playerToken, Metric, minPly)
        {
        }

        private static double Score(PlayerState<Move> playerState)
        {
            var state = (GameState)playerState.GameState;
            var board = state.Board;
            var playerColor = state.Players.IndexOf(playerState.PlayerToken) == 0
                ? Pieces.White
                : Pieces.Black;

            var score = 0;

            for (var i = board.Length - 1; i >= 0; i--)
            {
                var piece = board[i];
                if (piece != Pieces.None)
                {
                    var mul = (piece & PieceMasks.Colors) == playerColor ? 1 : -1;
                    switch (piece & PieceMasks.Piece)
                    {
                        case Pieces.Pawn:
                            score += mul;
                            break;

                        case Pieces.Knight:
                        case Pieces.Bishop:
                            score += mul * 3;
                            break;

                        case Pieces.Rook:
                            score += mul * 5;
                            break;

                        case Pieces.Queen:
                            score += mul * 9;
                            break;
                    }
                }
            }

            return score;
        }
    }
}
