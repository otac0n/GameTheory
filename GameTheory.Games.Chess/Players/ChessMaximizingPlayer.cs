// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Players
{
    using System;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Chess</see>.
    /// </summary>
    public class ChessMaximizingPlayer : MaximizingPlayer<GameState, Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<GameState, Move, double> Metric =
            ScoringMetric.Create<GameState, Move>(Score);

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChessMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public ChessMaximizingPlayer(PlayerToken playerToken, int minPly = 2, int thinkSeconds = 5, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc/>
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

        internal static double Score(PlayerState<GameState, Move> playerState)
        {
            var state = playerState.GameState;
            var playerColor = state.Players.IndexOf(playerState.PlayerToken) == 0
                ? Pieces.White
                : Pieces.Black;
            var promotionRank = state.Variant.PromotionRank[playerColor];

            var score = 0.0;

            for (var i = state.Variant.Size - 1; i >= 0; i--)
            {
                switch (state[i] ^ playerColor)
                {
                    case Pieces.Pawn:
                        var dist = Math.Abs(state.Variant.GetCoordinates(i).Y - promotionRank);
                        score += 2 - (dist / (state.Variant.Height - 3.0));
                        break;

                    case Pieces.Knight:
                    case Pieces.Bishop:
                        score += 3;
                        break;

                    case Pieces.Rook:
                        score += 5;
                        break;

                    case Pieces.Queen:
                        score += 9;
                        break;
                }
            }

            score += 0.1 * (state.ActiveColor == playerColor ? state : state.With(playerColor == Pieces.White ? Pieces.Black : Pieces.White)).GetAvailableMoves().Count;

            return score;
        }
    }
}
