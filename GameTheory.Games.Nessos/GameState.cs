// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using GameTheory.Games.Nessos.Moves;

    /// <summary>
    /// Represents the current state in a game of Nessos.
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
        public const int MinPlayers = 3;

        /// <summary>
        /// The maximum number of offered cards.
        /// </summary>
        public const int MaxOfferedCards = 3;

        /// <summary>
        /// The maximum number of cards in hand.
        /// </summary>
        public const int HandLimit = 5;

        /// <summary>
        /// The threshold of owned <see cref="Card.Charon"/> cards at which a player is elminated.
        /// </summary>
        public const int PlayerCharonLimit = 3;

        /// <summary>
        /// The threshold of owned <see cref="Card.Charon"/> cards at which the game ends.
        /// </summary>
        public const int TotalCharonLimit = 9;

        /// <summary>
        /// Gets the number of points needed to win the game.
        /// </summary>
        public static readonly ImmutableDictionary<int, int> PointThresholds = ImmutableDictionary.CreateRange(new Dictionary<int, int>
        {
            [3] = 40,
            [4] = 40,
            [5] = 35,
            [6] = 30,
        });

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
            this.FirstPlayer = this.Players[0];
            this.Phase = this.Phase;
            var deck = MakeDeck(players);
            this.OfferedCards = ImmutableList<OfferedCard>.Empty;
            var inventoryBuilder = ImmutableDictionary.CreateBuilder<PlayerToken, Inventory>();
            foreach (var player in this.Players)
            {
                deck = deck.Deal(HandLimit, out var startingHand);
                inventoryBuilder.Add(player, new Inventory().With(
                    hand: new EnumCollection<Card>(startingHand)));
            }
            this.Inventory = inventoryBuilder.ToImmutable();
            this.Deck = deck;
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            PlayerToken firstPlayer,
            Phase phase,
            EnumCollection<Card> deck,
            ImmutableList<OfferedCard> offeredCards,
            PlayerToken targetPlayer,
            ImmutableDictionary<PlayerToken, Inventory> inventory)
        {
            this.Players = players;
            this.FirstPlayer = firstPlayer;
            this.Phase = phase;
            this.Deck = deck;
            this.OfferedCards = offeredCards;
            this.TargetPlayer = targetPlayer;
            this.Inventory = inventory;
        }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> representing the first player.
        /// </summary>
        public PlayerToken FirstPlayer { get; }

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

            if ((comp = this.FirstPlayer.CompareTo(state.FirstPlayer)) != 0 ||
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

        /// <summary>
        /// Gets the Amphora <see cref="Deck"/> for the game.
        /// </summary>
        public EnumCollection<Card> Deck { get; }

        /// <summary>
        /// Gets the list of offered cards.
        /// </summary>
        public ImmutableList<OfferedCard> OfferedCards { get; }

        /// <summary>
        /// Gets the current target of the offered cards.
        /// </summary>
        public PlayerToken TargetPlayer { get; }

        /// <summary>
        /// Makes the <see cref="Deck"/> used for the current number of players.
        /// </summary>
        public static EnumCollection<Card> MakeDeck(int players)
        {
            switch (players)
            {
                case 3:
                    return EnumCollection<Card>.Empty
                        .Add(Card.Charon, 11)
                        .Add(Card.Satyr, 4)
                        .Add(Card.Centaur, 4)
                        .Add(Card.NemeanLion, 4)
                        .Add(Card.Griffin, 4)
                        .Add(Card.Pegasus, 4)
                        .Add(Card.LerneanHydra, 4)
                        .Add(Card.Medusa, 4);

                case 4:
                    return EnumCollection<Card>.Empty
                        .Add(Card.Charon, 14)
                        .Add(Card.Satyr, 4)
                        .Add(Card.Centaur, 4)
                        .Add(Card.NemeanLion, 4)
                        .Add(Card.Minotaur, 4)
                        .Add(Card.Griffin, 4)
                        .Add(Card.Pegasus, 4)
                        .Add(Card.Cerberus, 4)
                        .Add(Card.LerneanHydra, 4)
                        .Add(Card.Medusa, 4);

                case 5:
                case 6:
                    return EnumCollection<Card>.Empty
                        .Add(Card.Charon, 15)
                        .Add(Card.Satyr, 4)
                        .Add(Card.Centaur, 4)
                        .Add(Card.NemeanLion, 4)
                        .Add(Card.Minotaur, 4)
                        .Add(Card.Griffin, 4)
                        .Add(Card.Phoenix, 4)
                        .Add(Card.Pegasus, 4)
                        .Add(Card.Cerberus, 4)
                        .Add(Card.LerneanHydra, 4)
                        .Add(Card.Medusa, 4);;

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => this.CompareTo(obj as IGameState<Move>) == 0;

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = new List<Move>();

            switch (this.Phase)
            {
                case Phase.Offer:
                    moves.AddRange(AcceptOfferMove.GenerateMoves(this));
                    moves.AddRange(RejectOfferMove.GenerateMoves(this));
                    moves.AddRange(OfferCardMove.GenerateMoves(this));
                    break;

                case Phase.Draw:
                    moves.AddRange(DrawCardMove.GenerateMoves(this));
                    break;
            }

            return moves.ToImmutableList();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.FirstPlayer.GetHashCode());

            for (var i = 0; i < this.Players.Length; i++)
            {
                HashUtilities.Combine(ref hash, this.Players[i].GetHashCode());
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
            var shuffler = new GameShuffler<GameState>(this);

            var group = "FaceDown";
            shuffler.AddCollection(
                group,
                this.Deck,
                (state, value) => state.With(
                    deck: new EnumCollection<Card>(value)));

            foreach (var p in this.Players)
            {
                var player = p;

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
            }

            return shuffler.Take(maxStates);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return Array.Empty<PlayerToken>();
            }

            return this.Players
                .Where(p => this.Inventory[p].OwnedCards[Card.Charon] < PlayerCharonLimit)
                .AllMaxBy(p => Score(this.Inventory[p].OwnedCards))
                .AllMaxBy(p => this.Inventory[p].OwnedCards.Count);
        }

        public static int Score(EnumCollection<Card> ownedCards)
        {
            if (ownedCards[Card.Charon] >= PlayerCharonLimit)
            {
                return 0;
            }

            var satyrs = ownedCards[Card.Satyr];
            var centaurs = ownedCards[Card.Centaur];
            var nemeanLions = ownedCards[Card.NemeanLion];
            var score = Math.Min(satyrs, Math.Min(centaurs, nemeanLions)) * 10;
            foreach (var key in ownedCards.Keys)
            {
                score += ownedCards[key] * (int)key;
            }

            return score;
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
            PlayerToken firstPlayer = null,
            Phase? phase = null,
            EnumCollection<Card> deck = null,
            ImmutableList<OfferedCard> offeredCards = null,
            Maybe<PlayerToken> targetPlayer = default,
            ImmutableDictionary<PlayerToken, Inventory> inventory = null)
        {
            return new GameState(
                this.Players,
                firstPlayer ?? this.FirstPlayer,
                phase ?? this.Phase,
                deck ?? this.Deck,
                offeredCards ?? this.OfferedCards,
                targetPlayer.HasValue ? targetPlayer.Value : this.TargetPlayer,
                inventory ?? this.Inventory);
        }
    }
}
