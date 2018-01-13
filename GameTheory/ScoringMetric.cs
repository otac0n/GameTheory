// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides static methods for working with <see cref="IScoringMetric{TSubject, TScore}"/>.
    /// </summary>
    public static class ScoringMetric
    {
        /// <summary>
        /// Combines one or more scores using the specified weights.
        /// </summary>
        /// <param name="scores">The weighted scores to combine.</param>
        /// <returns>The combined score.</returns>
        public static double Combine(IWeighted<double>[] scores)
        {
            var weight = 0.0;
            var sum = 0.0;
            for (var i = scores.Length - 1; i >= 0; i--)
            {
                var score = scores[i];
                var w = score.Weight;
                sum += w * score.Value;
                weight += w;
            }

            return sum / weight;
        }

        /// <summary>
        /// Creates a new scoring metric given the specified definitions.
        /// </summary>
        /// <typeparam name="TSubject">The type of subject to be scored.</typeparam>
        /// <typeparam name="TScore">The type used to represent the score.</typeparam>
        /// <param name="score">Computes the score for the specified subject.</param>
        /// <param name="combine">Combines one or more scores using the specified weights.</param>
        /// <param name="comparison">Compares two scores and returns a value indicating whether one is less than, equal to, or greater than the other.</param>
        /// <returns>The scoring metric.</returns>
        public static IScoringMetric<TSubject, TScore> Create<TSubject, TScore>(Func<TSubject, TScore> score, Func<IWeighted<TScore>[], TScore> combine, Comparison<TScore> comparison) =>
            new FuncScoringMetric<TSubject, TScore>(score, combine, comparison);

        private class FuncScoringMetric<TSubject, TScore> : IScoringMetric<TSubject, TScore>
        {
            private readonly Func<IWeighted<TScore>[], TScore> combine;
            private readonly Comparison<TScore> comparison;
            private readonly Func<TSubject, TScore> score;

            public FuncScoringMetric(Func<TSubject, TScore> score, Func<IWeighted<TScore>[], TScore> combine, Comparison<TScore> comparison)
            {
                this.score = score;
                this.combine = combine;
                this.comparison = comparison;
            }

            TScore IScoringMetric<TSubject, TScore>.Combine(params IWeighted<TScore>[] scores) => this.combine(scores);

            int IComparer<TScore>.Compare(TScore x, TScore y) => this.comparison(x, y);

            TScore IScoringMetric<TSubject, TScore>.Score(TSubject subject) => this.score(subject);
        }
    }
}
