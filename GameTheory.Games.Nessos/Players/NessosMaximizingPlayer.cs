// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Players
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Players.MaximizingPlayer;

    /// <summary>
    /// Provides a maximizing player for the game of <see cref="GameState">Love Letter</see>.
    /// </summary>
    public class NessosMaximizingPlayer : MaximizingPlayer<Move, ResultScore<Tuple<double, double>>>
    {
        private static readonly IGameStateScoringMetric<Move, Tuple<double, double>> Metric = ScoringMetric.Create<Move, Tuple<double, double>>(
            Score,
            Combine,
            Compare,
            Difference);

        private TimeSpan minThinkTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="NessosMaximizingPlayer"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="minPly">The minimum number of ply to think ahead.</param>
        /// <param name="thinkSeconds">The minimum number of seconds to think.</param>
        /// <param name="misereMode">A value indicating whether or not to play misère.</param>
        public NessosMaximizingPlayer(PlayerToken playerToken, int minPly = 5, int thinkSeconds = 5, bool misereMode = false)
            : base(playerToken, ResultScoringMetric.Create(Metric, misereMode), minPly)
        {
            this.minThinkTime = TimeSpan.FromSeconds(Math.Max(1, thinkSeconds) - 0.1);
        }

        /// <inheritdoc/>
        protected override TimeSpan? MinThinkTime => this.minThinkTime;

        private static Tuple<double, double> Combine(params Weighted<Tuple<double, double>>[] scores)
        {
            var score = 0.0;
            var charon = 0.0;
            var totalWeight = 0.0;

            for (var i = 0; i < scores.Length; i++)
            {
                var p = scores[i];
                var s = p.Value;
                totalWeight += p.Weight;
                score += s.Item1 * p.Weight;
                charon += s.Item2 * p.Weight;
            }

            return Tuple.Create(score / totalWeight, charon / totalWeight);
        }

        private static int Compare(Tuple<double, double> x, Tuple<double, double> y)
        {
            var comp = x.Item1.CompareTo(y.Item1);
            return comp != 0 ? comp : y.Item2.CompareTo(x.Item2);
        }

        private static Tuple<double, double> Difference(Tuple<double, double> minuend, Tuple<double, double> subtrahend)
        {
            return Tuple.Create(minuend.Item1 - subtrahend.Item1, minuend.Item2 - subtrahend.Item2);
        }

        private static Tuple<double, double> Score(PlayerState<Move> playerState)
        {
            var ownedCards = ((GameState)playerState.GameState).Inventory[playerState.PlayerToken].OwnedCards;

            return Tuple.Create((double)GameState.Score(ownedCards), (double)ownedCards[Card.Charon]);
        }
    }
}
