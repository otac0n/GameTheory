// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides shared logic for maximizing players to prioritize winning over a higher score.
    /// </summary>
    /// <typeparam name="TMove">The type of move in the game state.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of the inner score.</typeparam>
    public sealed class ResultScoringMetric<TMove, TScore> : IGameStateScoringMetric<TMove, ResultScore<TScore>>, IScorePlyExtender<ResultScore<TScore>>
        where TMove : IMove
    {
        private readonly IScoringMetric<PlayerState<TMove>, TScore> scoringMetric;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultScoringMetric{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="scoringMetric">The inner scoring metric.</param>
        public ResultScoringMetric(IScoringMetric<PlayerState<TMove>, TScore> scoringMetric)
        {
            this.scoringMetric = scoringMetric;
        }

        /// <inheritdoc/>
        public ResultScore<TScore> Combine(params Weighted<ResultScore<TScore>>[] scores)
        {
            if (scores == null)
            {
                throw new ArgumentNullException(nameof(scores));
            }

            const int Weight = 0;
            const int Likelihood = 1;
            const int InPly = 2;
            const int FieldCount = 3;

            const int Offset = -(int)Result.Loss;
            const int ResultCount = Offset + (int)Result.Win + 1;
            var results = new double[ResultCount, FieldCount];
            for (var r = ResultCount - 1; r >= 0; r--)
            {
                results[r, InPly] = double.NaN;
            }

            var totalWeight = 0.0;
            var weightedRest = new Weighted<TScore>[scores.Length];
            for (var i = scores.Length - 1; i >= 0; i--)
            {
                var score = scores[i];
                var weight = score.Weight;
                var resultScore = score.Value;
                weightedRest[i] = Weighted.Create(resultScore.Rest, weight);
                totalWeight += weight;
                var r = (int)resultScore.Result + Offset;
                results[r, Weight] += weight;
                results[r, Likelihood] += resultScore.Likelihood * weight;
                results[r, InPly] = double.IsNaN(results[r, InPly]) || results[r, InPly].CompareTo(resultScore.InPly) > 0 ? resultScore.InPly : results[r, InPly]; // TODO: Should use PlyCountSortDirection?
            }

            var pessimisticResult = 0;
            for (; pessimisticResult < ResultCount; pessimisticResult++)
            {
                if (results[pessimisticResult, Weight] > 0)
                {
                    break;
                }
            }

            var rest = this.scoringMetric.Combine(weightedRest);

            return new ResultScore<TScore>((Result)(pessimisticResult - Offset), results[pessimisticResult, InPly], results[pessimisticResult, Likelihood] / totalWeight, rest);
        }

        /// <inheritdoc/>
        public int Compare(ResultScore<TScore> x, ResultScore<TScore> y)
        {
            int comp;
            if ((comp = EnumComparer<Result>.Default.Compare(x.Result, y.Result)) != 0)
            {
                return comp;
            }

            if ((comp = y.Likelihood.CompareTo(x.Likelihood)) != 0 ||
                (comp = x.InPly.CompareTo(y.InPly)) != 0)
            {
                return PlyCountSortDirection(x.Result, comp);
            }

            return this.scoringMetric.Compare(x.Rest, y.Rest);
        }

        /// <inheritdoc/>
        public ResultScore<TScore> Difference(ResultScore<TScore> minuend, ResultScore<TScore> subtrahend)
        {
            var result = minuend.Result;
            var rest = this.scoringMetric.Difference(minuend.Rest, subtrahend.Rest);
            var inPly = result != subtrahend.Result ? minuend.InPly : minuend.InPly - subtrahend.InPly;

            return new ResultScore<TScore>(result, inPly, double.NaN, rest);
        }

        /// <inheritdoc/>
        public ResultScore<TScore> Extend(ResultScore<TScore> score) => new ResultScore<TScore>(score.Result, score.InPly + 1, score.Likelihood, score.Rest);

        ResultScore<TScore> IScoringMetric<PlayerState<TMove>, ResultScore<TScore>>.Score(PlayerState<TMove> playerState)
        {
            var winners = playerState.GameState.GetWinners();
            var sharedResult = winners.Count == 0 ? (playerState.GameState.GetAvailableMoves().Count == 0 ? (playerState.GameState.Players.Count == 1 ? Result.Loss : Result.Impasse) : Result.None) : (Result?)null;
            var winnersSet = sharedResult == null ? winners.ToSet() : null;
            var result = sharedResult ?? (winnersSet.Contains(playerState.PlayerToken) ? (winnersSet.Count == 1 ? Result.Win : Result.SharedWin) : Result.Loss);
            return new ResultScore<TScore>(result, 0, 1, this.scoringMetric.Score(playerState));
        }

        /// <inheritdoc/>
        public IDictionary<PlayerToken, ResultScore<TScore>> Score(IGameState<TMove> state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var winners = state.GetWinners();
            var sharedResult = winners.Count == 0 ? (state.GetAvailableMoves().Count == 0 ? (state.Players.Count == 1 ? Result.Loss : Result.Impasse) : Result.None) : (Result?)null;
            var winnersSet = sharedResult == null ? winners.ToSet() : null;
            return state.Players.ToDictionary(p => p, p =>
            {
                var result = sharedResult ?? (winnersSet.Contains(p) ? (winnersSet.Count == 1 ? Result.Win : Result.SharedWin) : Result.Loss);
                return new ResultScore<TScore>(result, 0, 1, this.scoringMetric.Score(new PlayerState<TMove>(p, state)));
            });
        }

        private static int PlyCountSortDirection(Result result, int comparison)
        {
            switch (result)
            {
                case Result.Win:
                case Result.SharedWin:
                    return comparison < 0 ? 1 :
                           comparison > 0 ? -1 :
                           0;

                case Result.Loss:
                case Result.Impasse:
                case Result.None:
                default:
                    return comparison;
            }
        }
    }
}
