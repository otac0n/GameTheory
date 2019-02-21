// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Represents the current state in a game of Chess.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        private ImmutableList<Move> allMovesCache;

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
            this.Board = ImmutableArray.CreateRange(board.Cast<Pieces>());
            this.PlyCountClock = plyCountClock;
            this.MoveNumber = moveNumber;
            this.EnPassantIndex = epCoordinate == null
                ? default(int?)
                : this.Variant.GetIndexOf(epCoordinate.Value.X, epCoordinate.Value.Y);
            this.Castling = castling == null
                ? ImmutableDictionary<Pieces, int>.Empty
                : castling.ToImmutableDictionary();
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            Variant variant,
            Pieces activeColor,
            ImmutableArray<Pieces> board,
            int plyCountClock,
            int moveNumber,
            int? enPassantIndex,
            ImmutableDictionary<Pieces, int> castling)
        {
            this.Players = players;
            this.Variant = variant;
            this.ActiveColor = activeColor;
            this.Board = board;
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
        /// Gets the board.
        /// </summary>
        public ImmutableArray<Pieces> Board { get; }

        /// <summary>
        /// Gets the castling rights.
        /// </summary>
        public ImmutableDictionary<Pieces, int> Castling { get; }

        /// <summary>
        /// Gets the index of the current en passant capture square.
        /// </summary>
        public int? EnPassantIndex { get; }

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

            // TODO: Compare castling rights.
            if ((comp = this.ActiveColor.CompareTo(state.ActiveColor)) != 0 ||
                (comp = this.MoveNumber.CompareTo(state.MoveNumber)) != 0 ||
                (comp = this.PlyCountClock.CompareTo(state.PlyCountClock)) != 0 ||
                (comp = this.EnPassantIndex.CompareTo(state.EnPassantIndex)) != 0 ||
                (comp = CompareUtilities.CompareEnumLists(this.Board, state.Board)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = this.Variant.GenerateMoves(this);

            return moves.ToImmutableList();
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
            return this.Board[this.Variant.GetIndexOf(x, y)];
        }

        /// <inheritdoc/>
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            yield return this;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<PlayerToken> GetWinners() => ImmutableList<PlayerToken>.Empty;

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

        internal ImmutableList<Move> GenerateAllMoves()
        {
            return this.allMovesCache ?? (this.allMovesCache = this.Variant.GenerateAllMoves(this).ToImmutableList());
        }

        internal GameState With(
            Pieces? activeColor = null,
            ImmutableArray<Pieces>? board = null,
            int? plyCountClock = null,
            int? moveNumber = null,
            int? enPassantIndex = null,
            ImmutableDictionary<Pieces, int> castling = null)
        {
            return new GameState(
                players: this.Players,
                variant: this.Variant,
                activeColor: activeColor ?? this.ActiveColor,
                board: board ?? this.Board,
                plyCountClock: plyCountClock ?? this.PlyCountClock,
                moveNumber: moveNumber ?? this.MoveNumber,
                enPassantIndex: enPassantIndex, // The en passant square is automatically reset.
                castling: castling ?? this.Castling);
        }
    }
}
