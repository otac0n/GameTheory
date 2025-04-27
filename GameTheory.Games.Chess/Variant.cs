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
        private const int BisohpDirection = 0x01;
        private const int RookDirection = 0x02;

        private static readonly Point[] BishopDirections = new[]
        {
            new Point(1, 1),
            new Point(-1, 1),
            new Point(-1, -1),
            new Point(1, -1),
        };

        private static readonly Point[] KinghtDirections = new[]
        {
            new Point(2, 1),
            new Point(-2, 1),
            new Point(-2, -1),
            new Point(2, -1),
            new Point(1, 2),
            new Point(-1, 2),
            new Point(-1, -2),
            new Point(1, -2),
        };

        private static readonly Point[] RookDirections = new[]
        {
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1),
        };

        private static readonly Pieces[] Sides = new[]
        {
            Pieces.Queen,
            Pieces.King,
        };

        private static readonly Point[][] StraightLines = new[]
        {
            Array.Empty<Point>(),
            BishopDirections,
            RookDirections,
            BishopDirections.Concat(RookDirections).ToArray(),
        };

        private static ImmutableDictionary<Point, Variant> variants = ImmutableDictionary<Point, Variant>.Empty;

        private readonly Dictionary<Pieces, int> castlingTargets;
        private readonly int[][] knightMoves;

        public Variant(int width, int height, bool? pawnsMayMoveTwoSquares = null, bool? enPassant = null, int drawingPlyCount = 100)
        {
            this.Width = width;
            this.Height = height;
            this.Size = width * height;
            this.PawnsMayMoveTwoSquares = this.Height >= 4 && (pawnsMayMoveTwoSquares ?? this.Height >= 6);
            this.EnPassant = this.PawnsMayMoveTwoSquares && (enPassant ?? this.PawnsMayMoveTwoSquares);
            this.DrawingPlyCount = drawingPlyCount;

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
            this.castlingTargets = new Dictionary<Pieces, int>
            {
                [Pieces.White | Pieces.Queen] = this.GetIndexOf(2, 0),
                [Pieces.White | Pieces.King] = this.GetIndexOf(this.Width - 2, 0),
                [Pieces.Black | Pieces.Queen] = this.GetIndexOf(2, this.Height - 1),
                [Pieces.Black | Pieces.King] = this.GetIndexOf(this.Width - 2, this.Height - 1),
            };
            this.knightMoves = (from index in Enumerable.Range(0, this.Size)
                                let coord = this.GetCoordinates(index)
                                select (from dir in Variant.KinghtDirections
                                        let x = dir.X + coord.X
                                        where x >= 0 && x < this.Width
                                        let y = dir.Y + coord.Y
                                        where y >= 0 && y < this.Height
                                        select this.GetIndexOf(x, y)).ToArray()).ToArray();
        }

        /// <summary>
        /// Gets the number of ply without a capture or pawn move before the game results in a draw.
        /// </summary>
        public int DrawingPlyCount { get; }

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

        /// <summary>
        /// Gets a value indicating whether or not the specified game state has insufficient material for checkmate.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> to inspect.</param>
        /// <returns><c>true</c>, if the game is a draw due to insufficient material; <c>false</c>, otherwise.</returns>
        public bool IsDrawingMaterial(GameState state)
        {
            var knights = 0;
            var bishopsEven = 0;
            var bishopsOdd = 0;
            for (var i = 0; i < this.Size; i++)
            {
                var piece = state.Board[i];
                var color = piece & (Pieces.White | Pieces.Black);
                switch (piece ^ color)
                {
                    case Pieces.Pawn:
                    case Pieces.Rook:
                    case Pieces.Queen:
                        return false;

                    case Pieces.Knight:
                        if (knights > 0 || bishopsEven > 0 || bishopsOdd > 0)
                        {
                            return false;
                        }

                        knights++;
                        break;

                    case Pieces.Bishop:
                        if (knights > 0)
                        {
                            return false;
                        }

                        var point = this.GetCoordinates(i);
                        if ((point.X + point.Y) % 2 == 0)
                        {
                            if (bishopsOdd > 0)
                            {
                                return false;
                            }

                            bishopsEven++;
                        }
                        else
                        {
                            if (bishopsEven > 0)
                            {
                                return false;
                            }

                            bishopsOdd++;
                        }

                        break;
                }
            }

            return true;
        }

        protected internal IReadOnlyList<Move> GenerateAllMoves(GameState state, Pieces activeColor, int? captureIndex = null)
        {
            var builder = new List<Move>();
            var width = this.Width;
            var pawnDirection = activeColor == Pieces.White ? 1 : -1;
            var promotionY = this.PromotionRank[activeColor];
            var pawnStartingY = this.PawnStartingRank[activeColor];

            for (var i = 0; i < this.Size; i++)
            {
                int targetX, targetY;
                var isKing = false;
                var directions = 0;
                var piece = state[i];

                switch (piece ^ activeColor)
                {
                    default:
                        continue;

                    case Pieces.Pawn:
                        var x = i % width;
                        var y = i / width;

                        // Move
                        targetY = y + pawnDirection;
                        var moveTarget = this.GetIndexOf(x, targetY);
                        if (!captureIndex.HasValue && state[moveTarget] == Pieces.None)
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
                                    if (state[moveTwoTarget] == Pieces.None)
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
                            if (captureIndex.HasValue && captureIndex != captureTarget)
                            {
                                continue;
                            }

                            var targetValue = state[captureTarget];
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

                        continue;

                    case Pieces.Knight:
                        foreach (var target in this.knightMoves[i])
                        {
                            if (!captureIndex.HasValue || target == captureIndex)
                            {
                                var targetValue = state[target];
                                if ((targetValue & activeColor) == Pieces.None)
                                {
                                    if (!captureIndex.HasValue || targetValue != Pieces.None)
                                    {
                                        builder.Add(new BasicMove(state, i, target));
                                    }
                                }
                            }
                        }

                        continue;

                    case Pieces.Bishop:
                        directions |= Variant.BisohpDirection;
                        break;

                    case Pieces.Rook:
                        directions |= Variant.RookDirection;
                        break;

                    case Pieces.King:
                        isKing = true;

                        if (!captureIndex.HasValue)
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
                                    if (t != i && t != rookIndex && state[t] != Pieces.None)
                                    {
                                        clear = false;
                                        break;
                                    }
                                    else if (t >= minCheck && t <= maxCheck)
                                    {
                                        var board = state.Board;
                                        board[i] = Pieces.None;
                                        board[t] = piece;
                                        var updated = state.With(
                                                activeColor: activeColor == Pieces.White ? Pieces.Black : Pieces.White,
                                                board: board);
                                        var checks = this.GenerateAllMoves(
                                            updated,
                                            updated.ActiveColor,
                                            captureIndex: t);
                                        if (checks.Any())
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
                        directions |= Variant.BisohpDirection;
                        directions |= Variant.RookDirection;
                        break;
                }

                {
                    var x = i % width;
                    var y = i / width;

                    foreach (var direction in StraightLines[directions])
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
                            var targetValue = state[moveTarget];
                            if (!captureIndex.HasValue || moveTarget == captureIndex)
                            {
                                if ((targetValue & activeColor) == Pieces.None)
                                {
                                    if (!captureIndex.HasValue || targetValue != Pieces.None)
                                    {
                                        builder.Add(new BasicMove(state, i, moveTarget));
                                    }
                                }
                            }

                            if (isKing || targetValue != Pieces.None)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return builder;
        }

        protected internal Tuple<IReadOnlyList<Move>, bool> GenerateMoves(GameState state)
        {
            var allMoves = state.GenerateAllMoves();
            var moves = new List<Move>(allMoves.Count);

            var activeColor = state.ActiveColor;
            var activeKing = Pieces.King | activeColor;
            foreach (var move in allMoves)
            {
                var result = move.Apply(state);
                var kingIndex = Enumerable.Range(0, this.Size).First(i => result[i] == activeKing);
                var kingChop = this.GenerateAllMoves(result, result.ActiveColor, kingIndex).Any();
                if (!kingChop)
                {
                    moves.Add(move);
                }
            }

            var isMate = moves.Count == 0;
            var isOtherDraw = !isMate && (state.PlyCountClock >= this.DrawingPlyCount || this.IsDrawingMaterial(state));
            if (isOtherDraw)
            {
                moves.Clear();
            }

            return Tuple.Create<IReadOnlyList<Move>, bool>(moves, isMate);
        }
    }
}
