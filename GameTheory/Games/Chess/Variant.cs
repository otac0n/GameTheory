// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using GameTheory.Games.Chess.Moves;
    using GameTheory.Games.Chess.Notation;

    /// <summary>
    /// Defines a chess variant.
    /// </summary>
    public class Variant
    {
        private static readonly ImmutableList<Point> BishopDirections = ImmutableList.Create(
            new Point(1, 1),
            new Point(-1, 1),
            new Point(-1, -1),
            new Point(1, -1));

        private static readonly ImmutableList<Point> KinghtDirections = ImmutableList.Create(
            new Point(2, 1),
            new Point(-2, 1),
            new Point(-2, -1),
            new Point(2, -1),
            new Point(1, 2),
            new Point(-1, 2),
            new Point(-1, -2),
            new Point(1, -2));

        private static readonly ImmutableList<Point> RookDirections = ImmutableList.Create(
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1));

        private static ImmutableDictionary<Point, Variant> variants = ImmutableDictionary<Point, Variant>.Empty;

        private readonly ImmutableDictionary<int, ImmutableList<int>> knightMoves;

        public Variant(int width, int height, bool? pawnsMayMoveTwoSquares = null, bool? enPassant = null)
        {
            this.Width = width;
            this.Height = height;
            this.Size = width * height;
            this.PawnsMayMoveTwoSquares = this.Height >= 4 && (pawnsMayMoveTwoSquares ?? this.Height >= 6);
            this.EnPassant = this.PawnsMayMoveTwoSquares && (enPassant ?? this.PawnsMayMoveTwoSquares);
            this.PromotionRank = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<Pieces, int>(Pieces.White, this.Height - 1),
                new KeyValuePair<Pieces, int>(Pieces.Black, 0),
            });
            this.PawnStartingRank = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<Pieces, int>(Pieces.White, 1),
                new KeyValuePair<Pieces, int>(Pieces.Black, this.Height - 2),
            });
            this.PromotablePieces = ImmutableList.Create(
                Pieces.Knight,
                Pieces.Bishop,
                Pieces.Rook,
                Pieces.Queen);

            this.NotationSystem = new LongAlgebraicNotation();
            this.knightMoves = (from index in Enumerable.Range(0, this.Size)
                                let coord = this.GetCoordinates(index)
                                select new KeyValuePair<int, ImmutableList<int>>(
                                    index,
                                    (from dir in Variant.KinghtDirections
                                     let x = dir.X + coord.X
                                     where x >= 0 && x < this.Width
                                     let y = dir.Y + coord.Y
                                     where y >= 0 && y < this.Height
                                     select this.GetIndexOf(x, y)).ToImmutableList())).ToImmutableDictionary();
        }

        /// <summary>
        /// Gets a value indicating whether paws may capture en passant.
        /// </summary>
        public bool EnPassant { get; }

        /// <summary>
        /// Gets the height of the board.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the notation system used for displaying moves.
        /// </summary>
        public NotationSystem NotationSystem { get; }

        /// <summary>
        /// Gets a value indicating whether pawns are allowed to move two spaces from their starting square.
        /// </summary>
        public bool PawnsMayMoveTwoSquares { get; }

        /// <summary>
        /// Gets the starting ranks for the pawns.
        /// </summary>
        public ImmutableDictionary<Pieces, int> PawnStartingRank { get; }

        /// <summary>
        /// Gets the list of promotable pieces.
        /// </summary>
        public ImmutableList<Pieces> PromotablePieces { get; }

        /// <summary>
        /// Gets the promotion ranks for the players.
        /// </summary>
        public ImmutableDictionary<Pieces, int> PromotionRank { get; }

        /// <summary>
        /// Gets the number of squares on the board.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the width of the board.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Creates the specified variant.
        /// </summary>
        /// <param name="width">The width of the board.</param>
        /// <param name="height">The height of the board.</param>
        /// <returns>The requrested variant.</returns>
        public static Variant Create(int width, int height)
        {
            var key = new Point(width, height);
            while (true)
            {
                var original = variants;
                if (original.TryGetValue(key, out var value))
                {
                    return value;
                }

                Interlocked.CompareExchange(
                    ref variants,
                    original.SetItem(key, new Variant(width, height)),
                    original);
            }
        }

        /// <summary>
        /// Gets the coordinate of the specified index.
        /// </summary>
        /// <param name="index">The index to look up.</param>
        /// <param name="x">The x-coordinate of the index.</param>
        /// <param name="y">The y-coordinate of the index.</param>
        public void GetCoordinates(int index, out int x, out int y)
        {
            x = index % this.Width;
            y = index / this.Width;
        }

        /// <summary>
        /// Gets the coordinate of the specified index.
        /// </summary>
        /// <param name="index">The index to look up.</param>
        /// <returns>The corresponding coordinate.</returns>
        public Point GetCoordinates(int index) => new Point(
            index % this.Width,
            index / this.Width);

        /// <summary>
        /// Gets the index given by the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the index.</param>
        /// <param name="y">The y-coordinate of the index.</param>
        /// <returns>The corresponding index.</returns>
        public int GetIndexOf(int x, int y) => y * this.Width + x;

        protected internal IEnumerable<Move> GenerateAllMoves(GameState state)
        {
            var board = state.Board;
            var activeColor = state.ActiveColor;
            var pawnDirection = activeColor == Pieces.White ? 1 : -1;
            var promotionY = this.PromotionRank[activeColor];
            var pawnStartingY = this.PawnStartingRank[activeColor];

            for (var i = 0; i < this.Size; i++)
            {
                int targetX, targetY;
                var isKing = false;
                var directions = ImmutableList<Point>.Empty;
                this.GetCoordinates(i, out var x, out var y);
                var piece = board[i];

                switch (piece ^ activeColor)
                {
                    case Pieces.Pawn:
                        // Move
                        targetY = y + pawnDirection;
                        var moveTarget = this.GetIndexOf(x, targetY);
                        if (board[moveTarget] == Pieces.None)
                        {
                            // Move and promote
                            if (targetY == promotionY)
                            {
                                foreach (var promotable in this.PromotablePieces)
                                {
                                    yield return new PromotionMove(state, i, moveTarget, promotable | activeColor);
                                }
                            }
                            else
                            {
                                yield return new BasicMove(state, i, moveTarget);

                                // Move 2 sqaures
                                if (y == pawnStartingY && this.PawnsMayMoveTwoSquares)
                                {
                                    var moveTwoTarget = this.GetIndexOf(x, targetY + pawnDirection);
                                    if (board[moveTwoTarget] == Pieces.None)
                                    {
                                        yield return this.EnPassant
                                            ? new TwoSquareMove(state, i, moveTwoTarget, moveTarget)
                                            : new BasicMove(state, i, moveTwoTarget);
                                    }
                                }
                            }
                        }

                        for (targetX = x - 1; targetX <= x + 1 && targetX < this.Width; targetX += 2)
                        {
                            if (targetX < 0)
                            {
                                continue;
                            }

                            var captureTarget = this.GetIndexOf(targetX, targetY);

                            var targetValue = board[captureTarget];
                            if (targetValue == Pieces.None)
                            {
                                if (state.EnPassantIndex == captureTarget)
                                {
                                    yield return new EnPassantCaptureMove(state, i, captureTarget, this.GetIndexOf(targetX, y));
                                }
                            }
                            else if ((targetValue & activeColor) == Pieces.None)
                            {
                                if (targetY == promotionY)
                                {
                                    foreach (var promotable in this.PromotablePieces)
                                    {
                                        yield return new PromotionMove(state, i, captureTarget, promotable | activeColor);
                                    }
                                }
                                else
                                {
                                    yield return new BasicMove(state, i, captureTarget);
                                }
                            }
                        }

                        break;

                    case Pieces.Knight:
                        foreach (var target in this.knightMoves[i])
                        {
                            if ((board[target] & activeColor) == Pieces.None)
                            {
                                yield return new BasicMove(state, i, target);
                            }
                        }

                        break;

                    case Pieces.Bishop:
                        directions = directions.AddRange(Variant.BishopDirections);
                        break;

                    case Pieces.Rook:
                        directions = directions.AddRange(Variant.RookDirections);
                        break;

                    case Pieces.King:
                        isKing = true;
                        goto case Pieces.Queen;

                    case Pieces.Queen:
                        directions = directions.AddRange(Variant.BishopDirections);
                        directions = directions.AddRange(Variant.RookDirections);
                        break;
                }

                foreach (var direction in directions)
                {
                    targetX = x;
                    targetY = y;
                    while (true)
                    {
                        targetX += direction.X;
                        targetY += direction.Y;
                        if (targetX < 0 || targetX >= this.Width ||
                            targetY < 0 || targetY >= this.Height)
                        {
                            break;
                        }

                        var moveTarget = this.GetIndexOf(targetX, targetY);
                        var targetValue = board[moveTarget];
                        if ((targetValue & activeColor) == Pieces.None)
                        {
                            yield return new BasicMove(state, i, moveTarget);
                        }

                        if (isKing || targetValue != Pieces.None)
                        {
                            break;
                        }
                    }
                }
            }
        }

        protected internal IEnumerable<Move> GenerateMoves(GameState state)
        {
            var allMoves = state.GenerateAllMoves();

            var activeColor = state.ActiveColor;
            var activeKing = Pieces.King | activeColor;
            foreach (var move in allMoves)
            {
                var result = move.Apply(state);
                var kingChops = result.GenerateAllMoves().OfType<BasicMove>().Where(basicMove => result.Board[basicMove.ToIndex] == activeKing);
                if (!kingChops.Any())
                {
                    yield return move;
                }
            }
        }
    }
}
