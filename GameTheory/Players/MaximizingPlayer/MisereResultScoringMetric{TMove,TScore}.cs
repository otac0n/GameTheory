// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    /// <summary>
    /// A scoring metric that allows a <see cref="MaximizingPlayer{TMove, TScore}"/> to play misère.
    /// </summary>
    /// <typeparam name="TMove">The type of move in the game state.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of the inner score.</typeparam>
    public class MisereResultScoringMetric<TMove, TScore> : ResultScoringMetric<TMove, TScore>
        where TMove : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MisereResultScoringMetric{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="scoringMetric">The inner scoring metric.</param>
        public MisereResultScoringMetric(IScoringMetric<PlayerState<TMove>, TScore> scoringMetric)
            : base(new MisereScoringWrapper(scoringMetric))
        {
        }

        /// <inheritdoc/>
        public override int Compare(Result a, Result b) => -base.Compare(a, b);

        private class MisereScoringWrapper : IScoringMetric<PlayerState<TMove>, TScore>
        {
            private readonly IScoringMetric<PlayerState<TMove>, TScore> innerScoringMetric;

            public MisereScoringWrapper(IScoringMetric<PlayerState<TMove>, TScore> innerScoringMetric)
            {
                this.innerScoringMetric = innerScoringMetric;
            }

            public TScore Combine(params Weighted<TScore>[] scores) => this.innerScoringMetric.Combine(scores);

            public int Compare(TScore x, TScore y)
            {
                var comp = this.innerScoringMetric.Compare(x, y);
                return comp > int.MinValue ? -comp : int.MaxValue;
            }

            public TScore Difference(TScore minuend, TScore subtrahend) => this.innerScoringMetric.Difference(minuend, subtrahend);

            public TScore Score(PlayerState<TMove> subject) => this.innerScoringMetric.Score(subject);
        }
    }
}
