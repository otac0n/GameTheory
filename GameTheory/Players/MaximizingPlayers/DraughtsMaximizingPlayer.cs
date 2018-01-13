﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayers
{
    using System.Linq;
    using GameTheory.Games.Draughts;

    /// <summary>
    /// A maximizing player for the game of <see cref="GameState">Draughts</see>.
    /// </summary>
    public class DraughtsMaximizingPlayer : MaximizingPlayer<Move, double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DraughtsMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        public DraughtsMaximizingPlayer(PlayerToken playerToken, int minPly = 6)
            : base(playerToken, new ScoringMetric(), minPly)
        {
        }

        private class ScoringMetric : IScoringMetric
        {
            /// <inheritdoc/>
            public double CombineScores(IWeighted<double>[] scores) =>
                scores.Sum(s => s.Value * s.Weight) / scores.Sum(s => s.Weight);

            /// <inheritdoc/>
            public int Compare(double x, double y) =>
                x.CompareTo(y);

            /// <inheritdoc/>
            public double Difference(double playerScore, double opponentScore) =>
                playerScore - opponentScore;

            /// <inheritdoc/>
            public double Score(IGameState<Move> state, PlayerToken playerToken)
            {
                var gameState = (GameState)state;
                var board = gameState.Board;
                var playerIndex = gameState.Players.IndexOf(playerToken);
                var playerColor = (Piece)(1 << playerIndex);

                var score = 0.0;

                for (var i = 0; i < board.Length; i++)
                {
                    var square = board[i];
                    if (square.HasFlag(playerColor))
                    {
                        score += 1.0;
                        if (square.HasFlag(Piece.Crowned))
                        {
                            score += 0.75;
                        }
                    }
                }

                return score;
            }
        }
    }
}
