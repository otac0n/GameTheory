// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Players
{
    using System;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Lotus</see>.
    /// </summary>
    public class LotusMaximizingPlayer : MaximizingPlayer<GameState, Move, ResultScore<double>>
    {
        private static readonly IGameStateScoringMetric<GameState, Move, double> Metric =
            ScoringMetric.Create<GameState, Move>(Score);

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="LotusMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public LotusMaximizingPlayer(PlayerToken playerToken, int minPly = 3, int thinkSeconds = 5, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc />
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

        private static double Score(PlayerState<GameState, Move> playerState)
        {
            var state = playerState.GameState;
            double score = state.GetScore(playerState.PlayerToken);
            if (state.Phase != Phase.End)
            {
                foreach (var flower in state.Field.Values)
                {
                    if (!flower.Petals.IsEmpty)
                    {
                        var controllingPlayers = GameState.GetControllingPlayers(flower);
                        if (controllingPlayers.Contains(playerState.PlayerToken))
                        {
                            score += (double)flower.Petals.Count / controllingPlayers.Count;
                        }
                    }
                }
            }

            return score;
        }
    }
}
