// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Represents the current game state in a game of Skull.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 6;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        /// <summary>
        /// Gets the number of completed challenes needed to win the game.
        /// </summary>
        public static readonly int WinThreshold = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="players">The number of players.</param>
        public GameState([Range(MinPlayers, MaxPlayers)] int players = MinPlayers)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableArray();
            this.ActivePlayer = this.Players[1];
            this.Phase = Phase.AddingCards;
            this.Inventory = Enumerable.Range(0, players).ToImmutableDictionary(i => this.Players[i], i => new Inventory());
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableDictionary<PlayerToken, Inventory> inventory)
        {
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Inventory = inventory;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the inventory of all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, Inventory> Inventory { get; }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <inheritdoc />
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

            int comp;

            if ((comp = EnumComparer<Phase>.Default.Compare(this.Phase, state.Phase)) != 0 ||
                (comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0)
            {
                return comp;
            }

            if (this.Inventory != state.Inventory)
            {
                foreach (var player in this.Players)
                {
                    if ((comp = this.Inventory[player].CompareTo(state.Inventory[player])) != 0)
                    {
                        return comp;
                    }
                }
            }

            return 0;
        }

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = ImmutableList.CreateBuilder<Move>();

            switch (this.Phase)
            {
                case Phase.AddingCards:
                    moves.AddRange(Moves.AddCardMove.GenerateMoves(this));
                    moves.AddRange(Moves.BidMove.GenerateMoves(this));
                    break;

                case Phase.Bidding:
                    moves.AddRange(Moves.BidMove.GenerateMoves(this));
                    moves.AddRange(Moves.PassMove.GenerateMoves(this));
                    break;

                case Phase.Challenge:
                    moves.AddRange(Moves.RevealCardsMove.GenerateMoves(this));
                    break;

                case Phase.ChooseDiscard:
                    moves.AddRange(Moves.ArrangeCardsMove.GenerateMoves(this));
                    moves.AddRange(Moves.RemoveCardMove.GenerateMoves(this));
                    moves.AddRange(Moves.DiscardCardMove.GenerateMoves(this));
                    break;

                case Phase.ChooseStartingPlayer:
                    moves.AddRange(Moves.ChooseStartingPlayerMove.GenerateMoves(this));
                    break;
            }

            return moves.ToImmutable();
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            var shuffler = new GameShuffler<GameState>(this);

            foreach (var p in this.Players)
            {
                var player = p;
                var group = player.Id.ToString();

                if (player == playerToken)
                {
                    continue;
                }

                shuffler.AddCollection(
                    group,
                    this.Inventory[player].Hand,
                    (state, value) => state.With(
                        inventory: state.Inventory.SetItem(
                            player,
                            state.Inventory[player].With(hand: new EnumCollection<Card>(value)))));

                shuffler.AddCollection(
                    group,
                    this.Inventory[player].Stack,
                    (state, value) => state.With(
                        inventory: state.Inventory.SetItem(
                            player,
                            state.Inventory[player].With(stack: value.ToImmutableList()))));

                shuffler.AddCollection(
                    group,
                    this.Inventory[player].Discards,
                    (state, value) => state.With(
                        inventory: state.Inventory.SetItem(
                            player,
                            state.Inventory[player].With(discards: new EnumCollection<Card>(value)))));
            }

            return shuffler.Take(maxStates);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            var remainingPlayers = this.Inventory.Where(i => i.Value.Bid > Skull.Inventory.PassingBid).Select(i => i.Key).ToImmutableList();
            if (remainingPlayers.Count == 1)
            {
                return remainingPlayers;
            }
            else
            {
                return remainingPlayers.Where(p => this.Inventory[p].ChallengesWon >= GameState.WinThreshold).ToImmutableList();
            }
        }

        /// <inheritdoc />
        public IGameState<Move> MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (this.CompareTo(move.GameState) != 0)
            {
                var equivalentMove = this.GetAvailableMoves().Where(m => m.CompareTo(move) == 0).FirstOrDefault();
                if (equivalentMove != null)
                {
                    move = equivalentMove;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(move));
                }
            }

            return move.Apply(this);
        }

        internal GameState With(
            PlayerToken activePlayer = null,
            Phase? phase = null,
            ImmutableDictionary<PlayerToken, Inventory> inventory = null)
        {
            return new GameState(
                this.Players,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                inventory ?? this.Inventory);
        }
    }
}
