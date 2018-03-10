// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GameTheory.Games.Draughts.Moves;

    /// <summary>
    /// Represents the impact of choosing a move that doesn't maximize the move priority.
    /// </summary>
    public enum MovePriorityImpact : byte
    {
        /// <summary>
        /// No impact.  Used in games where captures are optional.
        /// </summary>
        None = 0,

        /// <summary>
        /// The opponent may remove any piece that may have made the highest priority move.
        /// </summary>
        PieceRemoval = 1,

        /// <summary>
        /// The move is not allowed.
        /// </summary>
        IllegalMove = 2,
    }

    /// <summary>
    /// Provides common move priorities.
    /// </summary>
    public static class MovePriorities
    {
        /// <summary>
        /// Gets a comparer that enforces the longest available capturing sequence.
        /// </summary>
        public static IComparer<Move> LongestCaptureSequence { get; } = MakeMaximizingComparer(
            c => 1,
            scores => scores.Sum(s => s.Value),
            (a, b) => a.CompareTo(b));

        /// <summary>
        /// Gets a comparer that enforces the longest available capturing sequence, breaking ties with the sequence containing the most captured kings.
        /// </summary>
        public static IComparer<Move> LongestCaptureSequenceCapturingMostKings { get; } = MakeMaximizingComparer(
            c => new
            {
                Pieces = 1,
                Kings = (c.GameState.Board[c.CaptureIndex] & Piece.Crowned) == Piece.Crowned ? 1 : 0,
            },
            scores => new
            {
                Pieces = scores.Sum(s => s.Value.Pieces),
                Kings = scores.Sum(s => s.Value.Kings),
            },
            (a, b) =>
            {
                int comp;
                if ((comp = a.Pieces.CompareTo(b.Pieces)) != 0 ||
                    (comp = a.Kings.CompareTo(b.Kings)) != 0)
                {
                    return comp;
                }

                return 0;
            });

        /// <summary>
        /// Gets a comparer that enforces the longest available capturing sequence, breaking ties with sequences using a king then the sequence containing the most captured kings.
        /// </summary>
        public static IComparer<Move> LongestCaptureSequenceByKingCapturingMostKings { get; } = MakeMaximizingComparer(
            c => new
            {
                Pieces = 1,
                WithKing = (c.GameState.Board[c.FromIndex] & Piece.Crowned) == Piece.Crowned,
                Kings = (c.GameState.Board[c.CaptureIndex] & Piece.Crowned) == Piece.Crowned ? 1 : 0,
            },
            scores => new
            {
                Pieces = scores.Sum(s => s.Value.Pieces),
                WithKing = scores.First().Value.WithKing,
                Kings = scores.Sum(s => s.Value.Kings),
            },
            (a, b) =>
            {
                int comp;
                if ((comp = a.Pieces.CompareTo(b.Pieces)) != 0 ||
                    (comp = a.WithKing.CompareTo(b.WithKing)) != 0 ||
                    (comp = a.Kings.CompareTo(b.Kings)) != 0)
                {
                    return comp;
                }

                // TODO: The sequence that captures a king first should have a higher priority.
                return 0;
            });

        /// <summary>
        /// Gets a comparer that enforces captures.
        /// </summary>
        public static IComparer<Move> MustCapture { get; } = Comparer<Move>.Create(
            (a, b) => (a is CaptureMove).CompareTo(b is CaptureMove));

        /// <summary>
        /// Creates a comparer that uses the specified scoring metrics to evaluate moves.
        /// </summary>
        /// <typeparam name="T">The type representing the score.</typeparam>
        /// <param name="scoreCapture">The scoring function to use.</param>
        /// <param name="combineScores">Adds two scores together.</param>
        /// <param name="scoreComparison">Compares two scores.</param>
        /// <returns>A comparer that evaluates moves based on their maximum potential score.</returns>
        public static IComparer<Move> MakeMaximizingComparer<T>(Func<CaptureMove, T> scoreCapture, Func<IWeighted<T>[], T> combineScores, Comparison<T> scoreComparison)
        {
            var scoringMetric = ScoringMetric.Create(scoreCapture, combineScores, scoreComparison, (a, b) => throw new NotImplementedException());
            return Comparer<Move>.Create((a, b) =>
            {
                var capA = a as CaptureMove;
                var capB = b as CaptureMove;

                int comp;
                if ((comp = (capA != null).CompareTo(capB != null)) != 0)
                {
                    return comp;
                }
                else if (capA == null)
                {
                    return 0;
                }

                if ((comp = capA.FromIndex.CompareTo(capB.FromIndex)) == 0 &&
                    (comp = capA.ToIndex.CompareTo(capB.ToIndex)) == 0 &&
                    (comp = capA.CaptureIndex.CompareTo(capB.CaptureIndex)) == 0)
                {
                    return 0;
                }

                Debug.Assert(capA.GameState.CompareTo(capB.GameState) == 0, "Moves are not comparable if not in the same state.");

                var state = capA.GameState;
                var player = state.ActivePlayer;
                var scoreA = MeasureCaptures(state, player, capA, scoringMetric);
                var scoreB = MeasureCaptures(state, player, capA, scoringMetric);

                return scoringMetric.Compare(scoreA, scoreB);
            });
        }

        /// <summary>
        /// Recursively searches capture moves to find the maxumum score.
        /// </summary>
        /// <typeparam name="T">The type representing the score.</typeparam>
        /// <param name="state">The state to measure.</param>
        /// <param name="player">The player whose turn is being evaluated.</param>
        /// <param name="move">The move to score.</param>
        /// <param name="scoringMetric">The scoring metric to use.</param>
        /// <returns>The maximum score possible.</returns>
        public static T MeasureCaptures<T>(GameState state, PlayerToken player, CaptureMove move, IScoringMetric<CaptureMove, T> scoringMetric)
        {
            var score = scoringMetric.Score(move);

            var nextState = (GameState)state.MakeMove(move);
            if (nextState.ActivePlayer == player)
            {
                score = scoringMetric.Combine(
                    Weighted.Create(score, 1),
                    Weighted.Create(CaptureMove.GenerateMoves(nextState).Select(m => MeasureCaptures(nextState, player, m, scoringMetric)).Max(scoringMetric), 1));
            }

            return score;
        }
    }
}
