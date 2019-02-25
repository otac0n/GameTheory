// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using GameTheory.Games.Chess.Moves;
    using GameTheory.Games.Chess.NotationSystems;

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

        private static readonly ImmutableList<Pieces> Sides = ImmutableList.Create(
            Pieces.Queen,
            Pieces.King);

        private static ImmutableDictionary<Point, Variant> variants = ImmutableDictionary<Point, Variant>.Empty;

        private readonly ImmutableDictionary<Pieces, int> castlingTargets;
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

            this.NotationSystem = new AlgebraicNotation();
            this.castlingTargets = ImmutableDictionary.CreateRange(new Dictionary<Pieces, int>
            {
                [Pieces.White | Pieces.Queen] = this.GetIndexOf(2, 0),
                [Pieces.White | Pieces.King] = this.GetIndexOf(this.Width - 2, 0),
                [Pieces.Black | Pieces.Queen] = this.GetIndexOf(2, this.Height - 1),
                [Pieces.Black | Pieces.King] = this.GetIndexOf(this.Width - 2, this.Height - 1),
            });
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

        protected internal ImmutableList<Move> GenerateAllMoves(GameState state, bool onlyCaptures = false)
        {
            var builder = ImmutableList.CreateBuilder<Move>();
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
                        if (!onlyCaptures && board[moveTarget] == Pieces.None)
                        {
                            // Move and promote
                            if (targetY == promotionY)
                            {
                                foreach (var promotable in this.PromotablePieces)
                                {
                                    builder.Add(new PromotionMove(state, i, moveTarget, promotable | activeColor));
                                }
                            }
                            else
                            {
                                builder.Add(new BasicMove(state, i, moveTarget));

                                // Move 2 sqaures
                                if (y == pawnStartingY && this.PawnsMayMoveTwoSquares)
                                {
                                    var moveTwoTarget = this.GetIndexOf(x, targetY + pawnDirection);
                                    if (board[moveTwoTarget] == Pieces.None)
                                    {
                                        builder.Add(this.EnPassant
                                            ? new TwoSquareMove(state, i, moveTwoTarget, moveTarget)
                                            : new BasicMove(state, i, moveTwoTarget));
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
                                    builder.Add(new EnPassantCaptureMove(state, i, captureTarget, this.GetIndexOf(targetX, y)));
                                }
                            }
                            else if ((targetValue & activeColor) == Pieces.None)
                            {
                                if (targetY == promotionY)
                                {
                                    foreach (var promotable in this.PromotablePieces)
                                    {
                                        builder.Add(new PromotionMove(state, i, captureTarget, promotable | activeColor));
                                    }
                                }
                                else
                                {
                                    builder.Add(new BasicMove(state, i, captureTarget));
                                }
                            }
                        }

                        break;

                    case Pieces.Knight:
                        foreach (var target in this.knightMoves[i])
                        {
                            var targetValue = board[target];
                            if ((targetValue & activeColor) == Pieces.None)
                            {
                                if (!onlyCaptures || targetValue != Pieces.None)
                                {
                                    builder.Add(new BasicMove(state, i, target));
                                }
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

                        if (!onlyCaptures)
                        {
                            foreach (var side in Variant.Sides)
                            {
                                var castle = side | activeColor;
                                var rookIndex = state.Castling[GameState.GetCastlingIndex(castle)];
                                if (rookIndex < 0)
                                {
                                    continue;
                                }

                                var target = this.castlingTargets[castle];
                                var rookTarget = target + (side == Pieces.Queen ? 1 : -1);

                                var minCheck = Math.Min(i, target);
                                var maxCheck = Math.Max(i, target);
                                var minClear = Math.Min(minCheck, Math.Min(rookIndex, rookTarget));
                                var maxClear = Math.Max(maxCheck, Math.Max(rookIndex, rookTarget));

                                var clear = true;
                                for (var t = minClear; t <= maxClear; t++)
                                {
                                    if (t != i && t != rookIndex && state.Board[t] != Pieces.None)
                                    {
                                        clear = false;
                                        break;
                                    }
                                    else if (t >= minCheck && t <= maxCheck)
                                    {
                                        var check = this.GenerateAllMoves(
                                            state.With(
                                                activeColor: activeColor == Pieces.White ? Pieces.Black : Pieces.White,
                                                board: board
                                                    .SetItem(t, piece)
                                                    .SetItem(i, Pieces.None)),
                                            onlyCaptures: true)
                                            .OfType<BasicMove>()
                                            .Where(basicMove => basicMove.ToIndex == t);
                                        if (check.Any())
                                        {
                                            clear = false;
                                            break;
                                        }
                                    }
                                }

                                if (clear)
                                {
                                    builder.Add(new CastleMove(state, side, i, target, rookIndex, rookTarget));
                                }
                            }
                        }

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
                            if (!onlyCaptures || targetValue != Pieces.None)
                            {
                                builder.Add(new BasicMove(state, i, moveTarget));
                            }
                        }

                        if (isKing || targetValue != Pieces.None)
                        {
                            break;
                        }
                    }
                }
            }

            return builder.ToImmutable();
        }

        protected internal IEnumerable<Move> GenerateMoves(GameState state)
        {
            var allMoves = state.GenerateAllMoves();

            var activeColor = state.ActiveColor;
            var activeKing = Pieces.King | activeColor;
            foreach (var move in allMoves)
            {
                var result = move.Apply(state);
                var kingChop = false;
                foreach (var response in result.GenerateAllMoves())
                {
                    if (response is BasicMove basicMove)
                    {
                        if (result.Board[basicMove.ToIndex] == activeKing)
                        {
                            kingChop = true;
                            break;
                        }
                    }
                }

                if (!kingChop)
                {
                    yield return move;
                }
            }
        }
    }
}
