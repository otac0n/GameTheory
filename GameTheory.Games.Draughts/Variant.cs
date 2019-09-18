// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Describes the rules for a variant of <see cref="GameState">Draughts</see>.
    /// </summary>
    public sealed class Variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Variant"/> class.
        /// </summary>
        /// <param name="width">The number of squares along the width of the board.</param>
        /// <param name="height">The number of squares along the edge of the board.</param>
        /// <param name="rightHandCornerOccupied">Indicates whether or not the right hand corner for each player contains a piece.</param>
        /// <param name="flyingKings">Indicates whether or not crowned pieces may move any distance along unblocked diagonals before coming to a rest or capturing.</param>
        /// <param name="menCaptureBackwards">Indicates whether or not uncrowned pieces may capture backwards.</param>
        /// <param name="menCanCaptureKings">Indicates whether or not men can capture kings.</param>
        /// <param name="crownOnEntry">Indicates whether or not pieces are crowned as soon as they reach their last rank.</param>
        /// <param name="movePriority">The rules for prioritizing available moves.</param>
        /// <param name="movePriorityImpact">The impact of choosing a move that doesn't maximize the move priority.</param>
        public Variant(int width, int height, bool rightHandCornerOccupied = false, bool flyingKings = false, bool menCaptureBackwards = false, bool menCanCaptureKings = true, bool crownOnEntry = false, MovePriorityImpact movePriorityImpact = MovePriorityImpact.None, IComparer<Move> movePriority = null)
        {
            if (width <= 0 || width % 2 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            else if (height <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (movePriorityImpact != MovePriorityImpact.None && movePriority == null)
            {
                throw new ArgumentOutOfRangeException(nameof(movePriority));
            }

            this.Width = width;
            this.Height = height;
            this.RightHandCornerOccupied = rightHandCornerOccupied;
            this.FlyingKings = flyingKings;
            this.MenCaptureBackwards = menCaptureBackwards;
            this.MenCanCaptureKings = menCanCaptureKings;
            this.CrownOnEntry = crownOnEntry;
            this.MovePriorityImpact = movePriorityImpact;
            this.MovePriority = movePriority;

            var files = width / 2;
            var filledRows = height / 2 - 1;
            var board = new Pieces[files * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < files; x++)
                {
                    var i = y * files + x;
                    if (y < filledRows)
                    {
                        board[i] = Pieces.Black;
                    }
                    else if (height - 1 - y < filledRows)
                    {
                        board[i] = Pieces.White;
                    }
                }
            }

            this.InitialBoardState = board.ToImmutableArray();
        }

        /// <summary>
        /// Gets the American Checkers variant.
        /// </summary>
        public static Variant AmericanCheckers => Variant.EnglishDraughts;

        /// <summary>
        /// Gets the Canadian Checkers variant.
        /// </summary>
        public static Variant CanadianCheckers => new Variant(
            width: 12,
            height: 12,
            flyingKings: true,
            menCaptureBackwards: true,
            movePriority: MovePriorities.LongestCaptureSequence,
            movePriorityImpact: MovePriorityImpact.IllegalMove);

        /// <summary>
        /// Gets the Casual Checkers variant.
        /// </summary>
        public static Variant CasualCheckers { get; } = new Variant(
            width: 8,
            height: 8);

        /// <summary>
        /// Gets the English Draughts variant.
        /// </summary>
        public static Variant EnglishDraughts { get; } = new Variant(
            width: 8,
            height: 8,
            movePriority: MovePriorities.MustCapture,
            movePriorityImpact: MovePriorityImpact.IllegalMove);

        /// <summary>
        /// Gets the International Draughts variant.
        /// </summary>
        public static Variant InternationalDraughts => new Variant(
            width: 10,
            height: 10,
            flyingKings: true,
            menCaptureBackwards: true,
            movePriority: MovePriorities.LongestCaptureSequence,
            movePriorityImpact: MovePriorityImpact.IllegalMove);

        /// <summary>
        /// Gets the Italian Draughts variant.
        /// </summary>
        public static Variant ItalianDraughts => new Variant(
            width: 8,
            height: 8,
            menCanCaptureKings: false,
            movePriority: MovePriorities.LongestCaptureSequenceByKingCapturingMostKings,
            movePriorityImpact: MovePriorityImpact.IllegalMove);

        /// <summary>
        /// Gets the Pool Checkers variant.
        /// </summary>
        public static Variant PoolCheckers => new Variant(
            width: 8,
            height: 8,
            flyingKings: true,
            menCaptureBackwards: true,
            movePriority: MovePriorities.MustCapture,
            movePriorityImpact: MovePriorityImpact.IllegalMove);

        /// <summary>
        /// Gets the Russian Draughts variant.
        /// </summary>
        public static Variant RussianDraughts => new Variant(
            width: 8,
            height: 8,
            flyingKings: true,
            menCaptureBackwards: true,
            crownOnEntry: true,
            movePriority: MovePriorities.MustCapture,
            movePriorityImpact: MovePriorityImpact.IllegalMove);

        /// <summary>
        /// Gets the Spanish Draughts variant.
        /// </summary>
        public static Variant SpanishDraughts => new Variant(
            width: 8,
            height: 8,
            flyingKings: true,
            movePriority: MovePriorities.LongestCaptureSequenceCapturingMostKings,
            movePriorityImpact: MovePriorityImpact.IllegalMove);

        /// <summary>
        /// Gets a value indicating whether or not pieces are crowned as soon as they reach their last rank.
        /// </summary>
        public bool CrownOnEntry { get; }

        /// <summary>
        /// Gets a value indicating whether or not crowned pieces may move any distance along unblocked diagonals before coming to a rest or capturing.
        /// </summary>
        public bool FlyingKings { get; }

        /// <summary>
        /// Gets the number of squares along the edge of the board.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the initial contents of the board.
        /// </summary>
        public ImmutableArray<Pieces> InitialBoardState { get; }

        /// <summary>
        /// Gets a value indicating whether or not men can capture kings.
        /// </summary>
        public bool MenCanCaptureKings { get; }

        /// <summary>
        /// Gets a value indicating whether or not uncrowned pieces may capture backwards.
        /// </summary>
        public bool MenCaptureBackwards { get; }

        /// <summary>
        /// Gets the rules for prioritizing available moves.
        /// </summary>
        public IComparer<Move> MovePriority { get; }

        /// <summary>
        /// Gets the impact of choosing a move that doesn't maximize the move priority.
        /// </summary>
        public MovePriorityImpact MovePriorityImpact { get; }

        /// <summary>
        /// Gets a value indicating whether or not the right hand corner for each player contains a piece.
        /// </summary>
        public bool RightHandCornerOccupied { get; }

        /// <summary>
        /// Gets the number of squares along the width of the board.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Get the coordinates of the specified index.
        /// </summary>
        /// <param name="index">The index to convert.</param>
        /// <param name="x">The horizontal component of the vector.</param>
        /// <param name="y">The vertical component of the vector.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public void GetCoordinates(int index, out int x, out int y)
        {
            var stride = this.Width / 2;
            y = index / stride;
            x = (index % stride) * 2 + ((y % 2 == 0) == this.RightHandCornerOccupied ? 0 : 1);
        }

        /// <summary>
        /// Get the index of the specified coordinate.
        /// </summary>
        /// <param name="x">The file number.</param>
        /// <param name="y">The rank number.</param>
        /// <returns>The index of the specified piece or <c>-1</c> if the coordinates don't refer to a piece.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public int GetIndexOf(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.Width || y >= this.Height)
            {
                return -1;
            }

            var q = (x + y) % 2;
            var r = this.RightHandCornerOccupied ? 0 : 1;
            if (q != r)
            {
                return -1;
            }

            return y * (this.Width / 2) + (x / 2);
        }
    }
}
