// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    using System.Collections.Generic;
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
        /// Gets a comparer that enforces captures.
        /// </summary>
        public static IComparer<Move> MustCapture { get; } = Comparer<Move>.Create((a, b) => (a is CaptureMove).CompareTo(b is CaptureMove));
    }
}
