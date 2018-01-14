﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.Draughts.Moves;

    /// <summary>
    /// Represents the current state in a game of Draughts (Checkers).
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="variant">The variant of Draughts being played.</param>
        public GameState(Variant variant)
        {
            this.Variant = variant;
            this.Phase = Phase.Play;
            this.Players = ImmutableList.Create(new PlayerToken(), new PlayerToken());
            this.ActivePlayer = this.Players[0];
            this.Board = variant.InitialBoardState;
            this.MaxMovePieceIndexes = ImmutableSortedSet<int>.Empty;
            this.OpponentMayRemovePiece = false;
        }

        private GameState(Variant variant, Phase phase, ImmutableList<PlayerToken> players, PlayerToken activePlayer, ImmutableArray<Piece> board, Maybe<int> lastCapturingIndex, ImmutableSortedSet<int> maxMovePieceIndexes, bool opponentMayRemovePiece)
        {
            this.Variant = variant;
            this.Phase = phase;
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Board = board;
            this.LastCapturingIndex = lastCapturingIndex;
            this.MaxMovePieceIndexes = maxMovePieceIndexes;
            this.OpponentMayRemovePiece = opponentMayRemovePiece;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the current state of the board.
        /// </summary>
        public ImmutableArray<Piece> Board { get; }

        /// <summary>
        /// Gets the index of the square that contains the piece that made the most recent capture.
        /// </summary>
        public Maybe<int> LastCapturingIndex { get; }

        /// <summary>
        /// Gets the indexes that could have made the maximum length move.
        /// </summary>
        public ImmutableSortedSet<int> MaxMovePieceIndexes { get; }

        /// <summary>
        /// Gets a value indicating whether or not the opponend will be allowed to remove a piece.
        /// </summary>
        public bool OpponentMayRemovePiece { get; }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets a list of players in the current game state.
        /// </summary>
        public ImmutableList<PlayerToken> Players { get; }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the variant of <see cref="GameState">Draughts</see> this state belongs to.
        /// </summary>
        public Variant Variant { get; }

        /// <inheritdoc/>
        public int CompareTo(IGameState<Move> other)
        {
            if (other == this)
            {
                return 0;
            }

            var state = other as GameState;
            if (state == null)
            {
                return 1;
            }

            if (this.Variant != state.Variant)
            {
                throw new InvalidOperationException();
            }

            int comp;

            if ((comp = this.Phase.CompareTo(state.Phase)) != 0 ||
                (comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
                (comp = this.LastCapturingIndex.CompareTo(state.LastCapturingIndex)) != 0 ||
                (comp = this.OpponentMayRemovePiece.CompareTo(state.OpponentMayRemovePiece)) != 0 ||
                (comp = CompareUtilities.CompareEnumLists(this.Board, state.Board)) != 0 ||
                (comp = CompareUtilities.CompareValueLists(this.MaxMovePieceIndexes, state.MaxMovePieceIndexes)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = this.GetAllMoves();

            if (this.Variant.MovePriorityImpact == MovePriorityImpact.IllegalMove)
            {
                return moves.AllMax(this.Variant.MovePriority).ToImmutableList();
            }

            return moves.ToImmutableList();
        }

        /// <inheritdoc/>
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <inheritdoc/>
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            yield return this;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (!this.GetAvailableMoves().Any())
            {
                return ImmutableList.Create(this.Players[1 - this.Players.IndexOf(this.ActivePlayer)]);
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

            if (this.CompareTo(move.State) != 0)
            {
                throw new InvalidOperationException();
            }

            var state = this;
            if (this.Variant.MovePriorityImpact == MovePriorityImpact.PieceRemoval)
            {
                var priority = this.Variant.MovePriority;
                var maxMoves = this.GetAllMoves().AllMax(priority);

                if (!state.LastCapturingIndex.HasValue)
                {
                    state = state.With(
                        maxMovePieceIndexes: ImmutableSortedSet.CreateRange(maxMoves.OfType<CaptureMove>().Select(m => m.FromIndex)));
                }

                if (!state.OpponentMayRemovePiece && priority.Compare(maxMoves.First(), move) > 0)
                {
                    state = state.With(
                        opponentMayRemovePiece: true);
                }
            }

            return move.Apply(state);
        }

        internal IEnumerable<Move> GetAllMoves()
        {
            switch (this.Phase)
            {
                case Phase.Play:
                    foreach (var m in CaptureMove.GenerateMoves(this))
                    {
                        yield return m;
                    }

                    foreach (var m in YieldCaptureMove.GenerateMoves(this))
                    {
                        yield return m;
                    }

                    foreach (var m in BasicMove.GenerateMoves(this))
                    {
                        yield return m;
                    }

                    break;

                case Phase.RemovePiece:
                    foreach (var m in RemovePieceMove.GenerateMoves(this))
                    {
                        yield return m;
                    }

                    foreach (var m in YieldRemovePieceMove.GenerateMoves(this))
                    {
                        yield return m;
                    }

                    break;
            }
        }

        internal GameState With(
            Phase? phase = null,
            ImmutableList<PlayerToken> players = null,
            PlayerToken activePlayer = null,
            ImmutableArray<Piece>? board = null,
            Maybe<int>? lastCapturingIndex = null,
            ImmutableSortedSet<int> maxMovePieceIndexes = null,
            bool? opponentMayRemovePiece = null)
        {
            return new GameState(
                variant: this.Variant,
                phase: phase ?? this.Phase,
                players: players ?? this.Players,
                activePlayer: activePlayer ?? this.ActivePlayer,
                board: board ?? this.Board,
                lastCapturingIndex: lastCapturingIndex ?? this.LastCapturingIndex,
                maxMovePieceIndexes: maxMovePieceIndexes ?? this.MaxMovePieceIndexes,
                opponentMayRemovePiece: opponentMayRemovePiece ?? this.OpponentMayRemovePiece);
        }
    }
}
