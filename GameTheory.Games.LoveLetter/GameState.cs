// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of LoveLetter.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 4;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        /// <summary>
        /// The starting deck.
        /// </summary>
        public static EnumCollection<Card> StartingDeck = EnumCollection<Card>.Empty
            .Add(Card.Guard, 5)
            .Add(Card.Priest, 2)
            .Add(Card.Baron, 2)
            .Add(Card.Handmaid, 2)
            .Add(Card.Prince, 2)
            .Add(Card.King, 1)
            .Add(Card.Countess, 1)
            .Add(Card.Princess, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        /// <param name="players">The number of players.</param>
        public GameState([Range(MinPlayers, MaxPlayers)] int players = MinPlayers)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableArray();
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Play;

            var deck = StartingDeck;
            deck = deck.Deal(out var hidden);
            this.Hidden = hidden;

            if (players < 3)
            {
                deck = deck.Deal(3, out var inaccessible);
                this.Inaccessible = new EnumCollection<Card>(inaccessible);
            }
            else
            {
                this.Inaccessible = EnumCollection<Card>.Empty;
            }

            var inventories = ImmutableDictionary.CreateBuilder<PlayerToken, Inventory>();
            foreach (var player in this.Players)
            {
                deck = deck.Deal(out var startingHand);
                inventories.Add(
                    player,
                    new Inventory(
                        ImmutableArray.Create<Card>(startingHand)));
            }
            this.Inventories = inventories.ToImmutable();

            this.Deck = deck;
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            Card hidden,
            EnumCollection<Card> inaccessible,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableDictionary<PlayerToken, Inventory> inventories)
        {
            this.Players = players;
            this.Hidden = hidden;
            this.Inaccessible = inaccessible;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Inventories = inventories;
        }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> representing the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the deck.
        /// </summary>
        public EnumCollection<Card> Deck { get; }

        /// <summary>
        /// Gets hidden card.
        /// </summary>
        public Card Hidden { get; }

        /// <summary>
        /// Gets the inaccessible cards.
        /// </summary>
        public EnumCollection<Card> Inaccessible { get; }

        /// <summary>
        /// Gets the player inventories.
        /// </summary>
        public ImmutableDictionary<PlayerToken, Inventory> Inventories { get; }

        /// <summary>
        /// Gets the phase.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

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

            int comp;

            if ((comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
                (comp = this.Hidden.CompareTo(state.Hidden)) != 0 ||
                (comp = this.Inaccessible.CompareTo(state.Inaccessible)) != 0 ||
                (comp = CompareUtilities.CompareDictionaries(this.Inventories, state.Inventories)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0 ||
                (comp = this.Deck.CompareTo(state.Deck)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => this.CompareTo(obj as IGameState<Move>) == 0;

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            return ImmutableList<Move>.Empty;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.ActivePlayer.GetHashCode());

            for (var i = 0; i < this.Players.Length; i++)
            {
                HashUtilities.Combine(ref hash, this.Inventories[this.Players[i]].GetHashCode());
            }

            return hash;
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            yield return this;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners() => ImmutableList<PlayerToken>.Empty;

        /// <inheritdoc />
        public IGameState<Move> MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            throw new NotImplementedException();
        }
    }
}
