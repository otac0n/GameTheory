// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GameTheory.Games.Chess.Moves;

    /// <summary>
    /// Represents the current state in a game of Chess.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        private static readonly ImmutableArray<int> EmptyCastling = ImmutableArray.Create(-1, -1, -1, -1);

        private readonly Pieces[] board;
        private WeakReference<IReadOnlyList<Move>> allMovesCache;
        private WeakReference<IReadOnlyList<Move>> availableMovesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        public GameState()
            : this("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="position">The Forsyth–Edwards Notation representation of the position.</param>
        public GameState(string position)
        {
            if (string.IsNullOrEmpty(position))
            {
                throw new ArgumentNullException(nameof(position));
            }

            var index = 0;
            if ((!Parser.TryParseXFen(position, ref index, out var board, out var activePlayer, out var castling, out var epCoordinate, out var plyCountClock, out var moveNumber) &&
                !Parser.TryParseShredderFen(position, ref index, out board, out activePlayer, out castling, out epCoordinate, out plyCountClock, out moveNumber)) ||
                index != position.Length)
            {
                throw new ArgumentException(nameof(position));
            }

            this.Players = ImmutableArray.Create(new PlayerToken(), new PlayerToken());
            this.Variant = Variant.Create(
                width: board.GetLength(0),
                height: board.GetLength(1));
            this.ActiveColor = Pieces.White;
            this.board = board.Cast<Pieces>().ToArray();
            this.PlyCountClock = plyCountClock;
            this.MoveNumber = moveNumber;
            this.EnPassantIndex = epCoordinate == null
                ? default(int?)
                : this.Variant.GetIndexOf(epCoordinate.Value.X, epCoordinate.Value.Y);
            this.Castling = castling == null
                ? EmptyCastling
                : castling.Aggregate(EmptyCastling, (c, n) => c.SetItem(GetCastlingIndex(n.Key), this.Variant.GetIndexOf(n.Value, (n.Key & PieceMasks.Colors) == Pieces.White ? 0 : this.Variant.Height - 1)));
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            Variant variant,
            Pieces activeColor,
            Pieces[] board,
            int plyCountClock,
            int moveNumber,
            int? enPassantIndex,
            ImmutableArray<int> castling)
        {
            this.Players = players;
            this.Variant = variant;
            this.ActiveColor = activeColor;
            this.board = board;
            this.PlyCountClock = plyCountClock;
            this.MoveNumber = moveNumber;
            this.EnPassantIndex = enPassantIndex;
            this.Castling = castling;
        }

        /// <summary>
        /// Gets the active color.
        /// </summary>
        /// <value>
        /// One of <see cref="Pieces.White"/> or <see cref="Pieces.Black"/>.
        /// </value>
        public Pieces ActiveColor { get; }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer => this.Players[this.ActiveColor == Pieces.White ? 0 : 1];

        /// <summary>
        /// Gets a copy of the internal board.
        /// </summary>
        public Pieces[] Board
        {
            get
            {
                var length = this.board.Length;
                var board = new Pieces[length];
                Array.Copy(this.board, board, length);
                return board;
            }
        }

        /// <summary>
        /// Gets the castling rights.
        /// </summary>
        public ImmutableArray<int> Castling { get; }

        /// <summary>
        /// Gets the index of the current en passant capture square.
        /// </summary>
        public int? EnPassantIndex { get; }

        /// <summary>
        /// Gets a value indicating whether or not the active player is in check.
        /// </summary>
        public bool IsCheck
        {
            get
            {
                var king = this.ActiveColor | Pieces.King;
                var kingIndex = Enumerable.Range(0, this.Variant.Size).First(i => this.board[i] == king);
                var check = this.Variant.GenerateAllMoves(
                    this.With(
                        activeColor: this.ActiveColor == Pieces.White ? Pieces.Black : Pieces.White),
                    onlyCaptures: true)
                    .OfType<BasicMove>()
                    .Where(basicMove => basicMove.ToIndex == kingIndex);
                return check.Any();
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the active player is in checkmate.
        /// </summary>
        public bool IsCheckmate => this.GetAvailableMoves().Count == 0 && this.IsCheck;

        /// <summary>
        /// Gets a value indicating whether or not the active player is in stalemate.
        /// </summary>
        public bool IsStalemate => this.GetAvailableMoves().Count == 0 && !this.IsCheck;

        /// <summary>
        /// Gets the current move number.
        /// </summary>
        public int MoveNumber { get; }

        /// <summary>
        /// Gets a list of players in the current game state.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the number of ply since the last move or capture.
        /// </summary>
        /// <remarks>
        /// Used to enforce the 50-move rule.
        /// </remarks>
        public int PlyCountClock { get; }

        /// <summary>
        /// Gets the variant.
        /// </summary>
        public Variant Variant { get; }

        /// <summary>
        /// Gets the value of the specified square.
        /// </summary>
        /// <param name="index">The index of the sqaure.</param>
        /// <returns>The value of the specified sqaure.</returns>
        public Pieces this[int index] => this.board[index];

        /// <summary>
        /// Gets the index in the <see cref="Castling"/> collection corresponding to a specific color's specific side.
        /// </summary>
        /// <param name="pieces">The color and side of the board.</param>
        /// <returns>The index in the <see cref="Castling"/> collection.</returns>
        public static int GetCastlingIndex(Pieces pieces)
        {
            switch (pieces)
            {
                case Pieces.White | Pieces.Queen:
                    return 0;

                case Pieces.White | Pieces.King:
                    return 1;

                case Pieces.Black | Pieces.Queen:
                    return 2;

                case Pieces.Black | Pieces.King:
                    return 3;

                default:
                    return -1;
            }
        }

        public static ImmutableArray<int> RemoveCastling(ImmutableArray<int> castling, Pieces color)
        {
            var ix = GameState.GetCastlingIndex(color | Pieces.Queen);

            if (castling[ix] >= 0)
            {
                castling = castling.SetItem(ix, -1);
            }

            ix++;

            if (castling[ix] >= 0)
            {
                castling = castling.SetItem(ix, -1);
            }

            return castling;
        }

        public static ImmutableArray<int> RemoveCastling(ImmutableArray<int> castling, int index)
        {
            for (var i = 0; i < 4; i++)
            {
                if (castling[i] == index)
                {
                    return castling.SetItem(i, -1);
                }
            }

            return castling;
        }

        public static ImmutableArray<int> RemoveCastling(ImmutableArray<int> castling, int index1, int index2)
        {
            for (var i = 0; i < 4; i++)
            {
                var value = castling[i];
                if (value == index1 || value == index2)
                {
                    castling = castling.SetItem(i, -1);
                }
            }

            return castling;
        }

        /// <inheritdoc/>
        public int CompareTo(IGameState<Move> other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            var state = other as GameState;
            if (object.ReferenceEquals(state, null))
            {
                return 1;
            }

            if (this.Variant != state.Variant)
            {
                throw new InvalidOperationException();
            }

            int comp;

            if ((comp = this.ActiveColor.CompareTo(state.ActiveColor)) != 0 ||
                (comp = this.MoveNumber.CompareTo(state.MoveNumber)) != 0 ||
                (comp = this.PlyCountClock.CompareTo(state.PlyCountClock)) != 0 ||
                (comp = this.EnPassantIndex.CompareTo(state.EnPassantIndex)) != 0 ||
                (comp = CompareUtilities.CompareEnumLists(this.board, state.board)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0 ||
                (comp = CompareUtilities.CompareValueLists(this.Castling, state.Castling)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            return CachingUtils.WeakRefernceCache(ref this.availableMovesCache, () => this.Variant.GenerateMoves(this));
        }

        /// <inheritdoc/>
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <summary>
        /// Get the piece at the specified coordinate.
        /// </summary>
        /// <param name="x">The horizontal component of the vector.</param>
        /// <param name="y">The vertical component of the vector.</param>
        /// <returns>The index of the specified piece or <see cref="Pieces.None"/> if the coordinates don't refer to a piece.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x", Justification = "X is meaningful in the context of coordinates.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y", Justification = "Y is meaningful in the context of coordinates.")]
        public Pieces GetPieceAt(int x, int y)
        {
            return this.board[this.Variant.GetIndexOf(x, y)];
        }

        /// <inheritdoc/>
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            yield return this;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.IsCheckmate)
            {
                return ImmutableList.Create(this.ActiveColor == Pieces.White ? this.Players[1] : this.Players[0]);
            }

            return ImmutableList<PlayerToken>.Empty;
        }

        /// <inheritdoc/>
        public IGameState<Move> MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (this.CompareTo(move.GameState) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(move));
            }

            return move.Apply(this);
        }

        internal IReadOnlyList<Move> GenerateAllMoves()
        {
            return CachingUtils.WeakRefernceCache(ref this.allMovesCache, () => this.Variant.GenerateAllMoves(this));
        }

        internal GameState With(
            Pieces? activeColor = null,
            Pieces[] board = null,
            int? plyCountClock = null,
            int? enPassantIndex = null,
            ImmutableArray<int>? castling = null)
        {
            return new GameState(
                players: this.Players,
                variant: this.Variant,
                activeColor: activeColor ?? this.ActiveColor,
                board: board ?? this.board,
                plyCountClock: plyCountClock ?? this.PlyCountClock,
                moveNumber: activeColor != this.ActiveColor && activeColor == Pieces.White ? this.MoveNumber + 1 : this.MoveNumber,
                enPassantIndex: enPassantIndex, // The en passant square is automatically reset.
                castling: castling ?? this.Castling);
        }
    }
}
