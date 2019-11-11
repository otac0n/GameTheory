// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class WinCountScoringMetric<TGameState, TMove> : IGameStateScoringMetric<TGameState, TMove, WinCount>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <inheritdoc/>
        public WinCount Combine(params Weighted<WinCount>[] scores)
        {
            var newDenominator = ScoringMetric.Combine(Array.ConvertAll(scores, w => new Weighted<double>(w.Value.Simulations, w.Weight)));
            var newNumerator = ScoringMetric.Combine(Array.ConvertAll(scores, w => new Weighted<double>(w.Value.Wins * newDenominator / w.Value.Simulations, w.Weight)));
            return new WinCount(newNumerator, newDenominator);
        }

        /// <inheritdoc/>
        public int Compare(WinCount x, WinCount y) => x.Ratio.CompareTo(y.Ratio);

        /// <inheritdoc/>
        public WinCount Difference(WinCount minuend, WinCount subtrahend)
        {
            Debug.Assert(minuend.Simulations > 0, "Invalid argument (division by zero).");
            Debug.Assert(subtrahend.Simulations > 0, "Invalid argument (division by zero).");

            if (subtrahend.Wins == 0)
            {
                return minuend;
            }

            if (subtrahend.Simulations == minuend.Simulations)
            {
                return new WinCount(minuend.Wins - subtrahend.Wins, minuend.Simulations);
            }

            var newDenominator = (minuend.Simulations + subtrahend.Simulations) / 2;
            var newNumerator = minuend.Wins * newDenominator / minuend.Simulations - subtrahend.Wins * newDenominator / subtrahend.Simulations;
            return new WinCount(newNumerator, newDenominator);
        }

        /// <inheritdoc/>
        public IDictionary<PlayerToken, WinCount> Score(TGameState state)
        {
            var winners = state.GetWinners();

            var sharedScore = winners.Count == 0
                ? (state.GetAvailableMoves().Count == 0 && state.Players.Count == 1 ? 0 : 1.0 / state.Players.Count)
                : (double?)null;

            var winnersSet = winners.ToSet();
            return state.Players.ToDictionary(p => p, p =>
            {
                var score = sharedScore ?? (winners.Contains(p) ? 1.0 / winners.Count : 0);
                return new WinCount(score, 1);
            });
        }

        /// <inheritdoc/>
        public WinCount Score(PlayerState<TGameState, TMove> playerState)
        {
            double score;

            var state = playerState.GameState;
            var winners = state.GetWinners();
            if (winners.Count == 0)
            {
                if (state.GetAvailableMoves().Count == 0 && state.Players.Count == 1)
                {
                    score = 0;
                }
                else
                {
                    score = 1.0 / state.Players.Count;
                }
            }
            else
            {
                if (winners.Any(w => w == playerState.PlayerToken))
                {
                    score = 1.0 / winners.Count;
                }
                else
                {
                    score = 0;
                }
            }

            return new WinCount(score, 1);
        }
    }
}
