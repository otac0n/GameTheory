// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Players.MaximizingPlayers
{
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Draughts</see>.
    /// </summary>
    public class DraughtsMaximizingPlayer : MaximizingPlayer<Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<Move, double> Metric =
            ScoringMetric.Create<Move>(Score);

        /// <summary>
        /// Initializes a new instance of the <see cref="DraughtsMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public DraughtsMaximizingPlayer(PlayerToken playerToken, int minPly = 6, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
        }

        private static double Score(PlayerState<Move> playerState)
        {
            var state = (GameState)playerState.GameState;
            var board = state.Board;
            var playerIndex = state.Players.IndexOf(playerState.PlayerToken);
            var playerColor = (Pieces)(1 << playerIndex);

            var score = 0.0;

            for (var i = 0; i < board.Length; i++)
            {
                var square = board[i];
                if ((square & playerColor) == playerColor)
                {
                    score += 1.0;
                    if ((square & Pieces.Crowned) == Pieces.Crowned)
                    {
                        score += 0.75;
                    }
                }
            }

            return score;
        }
    }
}
