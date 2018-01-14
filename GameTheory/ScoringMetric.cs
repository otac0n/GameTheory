﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        /// <param name="difference">Gets the difference between two scores.</param>
        /// <returns>The scoring metric.</returns>
        public static IScoringMetric<TSubject, TScore> Create<TSubject, TScore>(Func<TSubject, TScore> score, Func<IWeighted<TScore>[], TScore> combine, Comparison<TScore> comparison, Func<TScore, TScore, TScore> difference) =>
            new FuncScoringMetric<TSubject, TScore>(score, combine, comparison, difference);

        /// <summary>
        /// Creates a new scoring metric given the specified scoring function that returns a double.
        /// </summary>
        /// <typeparam name="TSubject">The type of subject to be scored.</typeparam>
        /// <param name="score">Computes the score for the specified subject.</param>
        /// <returns>The scoring metric.</returns>
        public static IScoringMetric<TSubject, double> Create<TSubject>(Func<TSubject, double> score) =>
            ScoringMetric.Create(
                score,
                ScoringMetric.Combine,
                (x, y) => x.CompareTo(y),
                (a, b) => a - b);

        private class FuncScoringMetric<TSubject, TScore> : IScoringMetric<TSubject, TScore>
        {
            private readonly Func<IWeighted<TScore>[], TScore> combine;
            private readonly Comparison<TScore> comparison;
            private readonly Func<TScore, TScore, TScore> difference;
            private readonly Func<TSubject, TScore> score;

            public FuncScoringMetric(Func<TSubject, TScore> score, Func<IWeighted<TScore>[], TScore> combine, Comparison<TScore> comparison, Func<TScore, TScore, TScore> difference)
            {
                this.score = score;
                this.combine = combine;
                this.comparison = comparison;
                this.difference = difference;
            }

            TScore IScoringMetric<TSubject, TScore>.Combine(params IWeighted<TScore>[] scores) => this.combine(scores);

            int IComparer<TScore>.Compare(TScore x, TScore y) => this.comparison(x, y);

            TScore IScoringMetric<TSubject, TScore>.Difference(TScore minuend, TScore subtrahend) => this.difference(minuend, subtrahend);

            TScore IScoringMetric<TSubject, TScore>.Score(TSubject subject) => this.score(subject);
        }
    }
}