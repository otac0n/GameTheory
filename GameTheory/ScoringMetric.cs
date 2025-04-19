// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        public static double Combine(Weighted<double>[] scores)
        {
            ArgumentNullException.ThrowIfNull(scores);

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
        public static IScoringMetric<TSubject, TScore> Create<TSubject, TScore>(Func<TSubject, TScore> score, Func<Weighted<TScore>[], TScore> combine, Comparison<TScore> comparison, Func<TScore, TScore, TScore> difference) =>
            new FuncScoringMetric<TSubject, TScore>(score, combine, comparison, difference);

        /// <summary>
        /// Creates a new scoring metric given the specified definitions.
        /// </summary>
        /// <typeparam name="TGameState">The type of game state to score.</typeparam>
        /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
        /// <typeparam name="TScore">The type used to represent the score.</typeparam>
        /// <param name="score">Computes the score for the specified subject.</param>
        /// <param name="combine">Combines one or more scores using the specified weights.</param>
        /// <param name="comparison">Compares two scores and returns a value indicating whether one is less than, equal to, or greater than the other.</param>
        /// <param name="difference">Gets the difference between two scores.</param>
        /// <returns>The scoring metric.</returns>
        public static IGameStateScoringMetric<TGameState, TMove, TScore> Create<TGameState, TMove, TScore>(Func<PlayerState<TGameState, TMove>, TScore> score, Func<Weighted<TScore>[], TScore> combine, Comparison<TScore> comparison, Func<TScore, TScore, TScore> difference)
            where TGameState : IGameState<TMove>
            where TMove : IMove =>
            new FuncGameStateScoringMetric<TGameState, TMove, TScore>(score, combine, comparison, difference);

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

        /// <summary>
        /// Creates a new scoring metric given the specified scoring function that returns a double.
        /// </summary>
        /// <typeparam name="TGameState">The type of game state to score.</typeparam>
        /// <typeparam name="TMove">The type of moves in the game state.</typeparam>
        /// <param name="score">Computes the score for the specified subject.</param>
        /// <returns>The scoring metric.</returns>
        public static IGameStateScoringMetric<TGameState, TMove, double> Create<TGameState, TMove>(Func<PlayerState<TGameState, TMove>, double> score)
            where TGameState : IGameState<TMove>
            where TMove : IMove =>
            ScoringMetric.Create(
                score,
                ScoringMetric.Combine,
                (x, y) => x.CompareTo(y),
                (a, b) => a - b);

        /// <summary>
        /// Creates a new scoring metric given the specified scoring function that returns a double.
        /// </summary>
        /// <typeparam name="TSubject">The type of subject to be scored.</typeparam>
        /// <param name="score">Computes the score for the specified subject.</param>
        /// <returns>The scoring metric.</returns>
        public static IScoringMetric<TSubject, byte> Null<TSubject>() => new NullScoringMetric<TSubject>();

        private class FuncGameStateScoringMetric<TGameState, TMove, TScore> : IGameStateScoringMetric<TGameState, TMove, TScore>
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            private readonly Func<Weighted<TScore>[], TScore> combine;
            private readonly Comparison<TScore> comparison;
            private readonly Func<TScore, TScore, TScore> difference;
            private readonly Func<PlayerState<TGameState, TMove>, TScore> score;

            public FuncGameStateScoringMetric(Func<PlayerState<TGameState, TMove>, TScore> score, Func<Weighted<TScore>[], TScore> combine, Comparison<TScore> comparison, Func<TScore, TScore, TScore> difference)
            {
                this.score = score;
                this.combine = combine;
                this.comparison = comparison;
                this.difference = difference;
            }

            TScore IScoringMetric<PlayerState<TGameState, TMove>, TScore>.Combine(params Weighted<TScore>[] scores) => this.combine(scores);

            int IComparer<TScore>.Compare(TScore x, TScore y) => this.comparison(x, y);

            TScore IScoringMetric<PlayerState<TGameState, TMove>, TScore>.Difference(TScore minuend, TScore subtrahend) => this.difference(minuend, subtrahend);

            IDictionary<PlayerToken, TScore> IGameStateScoringMetric<TGameState, TMove, TScore>.Score(TGameState state)
            {
                ArgumentNullException.ThrowIfNull(state);

                return state.Players.ToDictionary(p => p, p => this.score(new PlayerState<TGameState, TMove>(p, state)));
            }

            TScore IScoringMetric<PlayerState<TGameState, TMove>, TScore>.Score(PlayerState<TGameState, TMove> subject) => this.score(subject);
        }

        private class FuncScoringMetric<TSubject, TScore> : IScoringMetric<TSubject, TScore>
        {
            private readonly Func<Weighted<TScore>[], TScore> combine;
            private readonly Comparison<TScore> comparison;
            private readonly Func<TScore, TScore, TScore> difference;
            private readonly Func<TSubject, TScore> score;

            public FuncScoringMetric(Func<TSubject, TScore> score, Func<Weighted<TScore>[], TScore> combine, Comparison<TScore> comparison, Func<TScore, TScore, TScore> difference)
            {
                this.score = score;
                this.combine = combine;
                this.comparison = comparison;
                this.difference = difference;
            }

            TScore IScoringMetric<TSubject, TScore>.Combine(params Weighted<TScore>[] scores) => this.combine(scores);

            int IComparer<TScore>.Compare(TScore x, TScore y) => this.comparison(x, y);

            TScore IScoringMetric<TSubject, TScore>.Difference(TScore minuend, TScore subtrahend) => this.difference(minuend, subtrahend);

            TScore IScoringMetric<TSubject, TScore>.Score(TSubject subject) => this.score(subject);
        }

        private class NullScoringMetric<TSubject> : IScoringMetric<TSubject, byte>
        {
            public byte Combine(params Weighted<byte>[] scores) => 0;

            public int Compare(byte x, byte y) => 0;

            public byte Difference(byte minuend, byte subtrahend) => 0;

            public byte Score(TSubject subject) => 0;
        }
    }
}
