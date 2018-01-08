// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GameTheory.Games.Draughts.Moves;

    /// <summary>
    /// Describes the impact of choosing a move that doesn't maximize the move priority.
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
        public static IComparer<Move> LongestCaptureSequence
        {
            get
            {
                return Comparer<Move>.Create((a, b) =>
                {
                    int comp;
                    if ((comp = (a is CaptureMove).CompareTo(b is CaptureMove)) != 0)
                    {
                        return comp;
                    }
                    else if (!(a is CaptureMove))
                    {
                        return 0;
                    }

                    var capA = (CaptureMove)a;
                    var capB = (CaptureMove)b;

                    if ((comp = capA.FromIndex.CompareTo(capB.FromIndex)) == 0 &&
                        (comp = capA.ToIndex.CompareTo(capB.ToIndex)) == 0 &&
                        (comp = capA.CaptureIndex.CompareTo(capB.CaptureIndex)) == 0)
                    {
                        return 0;
                    }

                    Debug.Assert(capA.State.CompareTo(capB.State) == 0, "Moves are not comparable if not in the same state.");

                    var state = capA.State;
                    var player = state.ActivePlayer;
                    var scoreA = MeasureCaptures(state, player, capA, c => 1);
                    var scoreB = MeasureCaptures(state, player, capA, c => 1);

                    return scoreA.CompareTo(scoreB);
                });
            }
        }

        /// <summary>
        /// Gets a comparer that enforces captures.
        /// </summary>
        public static IComparer<Move> MustCapture { get; } = Comparer<Move>.Create((a, b) => (a is CaptureMove).CompareTo(b is CaptureMove));

        /// <summary>
        /// Recursively searches capture moves to find the maxumum score.
        /// </summary>
        /// <param name="state">The state to measure.</param>
        /// <param name="player">The player whose turn is being evaluated.</param>
        /// <param name="move">The move to score.</param>
        /// <param name="scoreCapture">The scoring function to use.</param>
        /// <returns>The maximum score possible.</returns>
        public static int MeasureCaptures(GameState state, PlayerToken player, CaptureMove move, Func<CaptureMove, int> scoreCapture)
        {
            var score = scoreCapture(move);

            var nextState = (GameState)state.MakeMove(move);
            if (nextState.ActivePlayer == player)
            {
                score += CaptureMove.GenerateMoves(nextState).Select(m => MeasureCaptures(nextState, player, m, scoreCapture)).Max();
            }

            return score;
        }
    }
}
