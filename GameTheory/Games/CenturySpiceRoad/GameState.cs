// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.CenturySpiceRoad.MerchantCards;

    /// <summary>
    /// Represents the current state of a game of Century Spice Road.
    /// </summary>
    public class GameState : IGameState<Move>
    {
        /// <summary>
        /// The maxumum number of spices allowed in a player's caravan.
        /// </summary>
        public const int CaravanLimit = 10;

        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 5;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        /// <summary>
        /// The point card count at which the game ends.
        /// </summary>
        public const int PointCardLimit = 5;

        private static readonly ImmutableList<MerchantCard> InitialHand;
        private static readonly ImmutableList<MerchantCard> InitialMerchantCards;
        private static readonly ImmutableList<PointCard> InitialPointCards;
        private static readonly ImmutableList<EnumCollection<Spice>> InitialSpices;

        static GameState()
        {
            InitialSpices = ImmutableList.Create(
                new EnumCollection<Spice>(Spice.Turmeric, Spice.Turmeric, Spice.Turmeric),
                new EnumCollection<Spice>(Spice.Turmeric, Spice.Turmeric, Spice.Turmeric, Spice.Turmeric),
                new EnumCollection<Spice>(Spice.Turmeric, Spice.Turmeric, Spice.Turmeric, Spice.Turmeric),
                new EnumCollection<Spice>(Spice.Turmeric, Spice.Turmeric, Spice.Turmeric, Spice.Saffron),
                new EnumCollection<Spice>(Spice.Turmeric, Spice.Turmeric, Spice.Turmeric, Spice.Saffron));
            InitialHand = ImmutableList.Create<MerchantCard>(
                new UpgradeCard(2),
                new SpiceCard(turmeric: 2));
            InitialPointCards = ImmutableList.Create(
                new PointCard(6, 2, 2, 0, 0),
                new PointCard(7, 3, 2, 0, 0),
                new PointCard(8, 2, 3, 0, 0),
                new PointCard(8, 0, 4, 0, 0),
                new PointCard(8, 2, 0, 2, 0),
                new PointCard(9, 3, 0, 2, 0),
                new PointCard(9, 2, 1, 0, 1),
                new PointCard(10, 0, 5, 0, 0),
                new PointCard(10, 0, 2, 2, 0),
                new PointCard(10, 2, 0, 0, 2),
                new PointCard(11, 2, 0, 3, 0),
                new PointCard(11, 3, 0, 0, 2),
                new PointCard(12, 0, 3, 2, 0),
                new PointCard(12, 0, 0, 4, 0),
                new PointCard(12, 1, 1, 1, 1),
                new PointCard(12, 0, 2, 1, 1),
                new PointCard(12, 1, 0, 2, 1),
                new PointCard(12, 0, 2, 0, 2),
                new PointCard(13, 2, 2, 2, 0),
                new PointCard(13, 0, 2, 3, 0),
                new PointCard(14, 3, 1, 1, 1),
                new PointCard(14, 0, 3, 0, 2),
                new PointCard(14, 0, 0, 2, 2),
                new PointCard(14, 2, 0, 0, 3),
                new PointCard(15, 0, 0, 5, 0),
                new PointCard(15, 2, 2, 0, 2),
                new PointCard(16, 1, 3, 1, 1),
                new PointCard(16, 0, 2, 3, 0),
                new PointCard(16, 0, 0, 0, 4),
                new PointCard(17, 2, 0, 2, 2),
                new PointCard(17, 0, 0, 3, 2),
                new PointCard(18, 1, 1, 3, 1),
                new PointCard(18, 0, 0, 2, 3),
                new PointCard(19, 0, 2, 2, 2),
                new PointCard(20, 1, 1, 1, 3),
                new PointCard(20, 0, 0, 0, 5));
            InitialMerchantCards = ImmutableList.Create<MerchantCard>(
                new SpiceCard(3, 0, 0, 0),
                new SpiceCard(4, 0, 0, 0),
                new SpiceCard(1, 1, 0, 0),
                new SpiceCard(1, 2, 0, 0),
                new SpiceCard(0, 2, 0, 0),
                new SpiceCard(0, 0, 1, 0),
                new SpiceCard(1, 0, 1, 0),
                new SpiceCard(0, 0, 0, 1),
                new UpgradeCard(3),
                new TradeCard(2, 0, 0, 0, 0, 2, 0, 0),
                new TradeCard(2, 0, 0, 0, 0, 0, 1, 0),
                new TradeCard(3, 0, 0, 0, 0, 3, 0, 0),
                new TradeCard(3, 0, 0, 0, 0, 1, 1, 0),
                new TradeCard(3, 0, 0, 0, 0, 0, 0, 1),
                new TradeCard(4, 0, 0, 0, 0, 0, 2, 0),
                new TradeCard(4, 0, 0, 0, 0, 0, 1, 1),
                new TradeCard(5, 0, 0, 0, 0, 0, 3, 0),
                new TradeCard(5, 0, 0, 0, 0, 0, 0, 2),
                new TradeCard(0, 1, 0, 0, 3, 0, 0, 0),
                new TradeCard(1, 1, 0, 0, 0, 0, 0, 1),
                new TradeCard(0, 2, 0, 0, 3, 0, 1, 0),
                new TradeCard(0, 2, 0, 0, 0, 0, 2, 0),
                new TradeCard(0, 2, 0, 0, 2, 0, 0, 1),
                new TradeCard(0, 3, 0, 0, 2, 0, 2, 0),
                new TradeCard(0, 3, 0, 0, 0, 0, 3, 0),
                new TradeCard(0, 3, 0, 0, 1, 0, 1, 1),
                new TradeCard(0, 3, 0, 0, 0, 0, 0, 2),
                new TradeCard(0, 0, 1, 0, 4, 1, 0, 0),
                new TradeCard(0, 0, 1, 0, 0, 2, 0, 0),
                new TradeCard(0, 0, 1, 0, 1, 2, 0, 0),
                new TradeCard(2, 0, 1, 0, 0, 0, 0, 2),
                new TradeCard(0, 0, 2, 0, 2, 3, 0, 0),
                new TradeCard(0, 0, 2, 0, 2, 1, 0, 1),
                new TradeCard(0, 0, 2, 0, 0, 2, 0, 1),
                new TradeCard(0, 0, 2, 0, 0, 0, 0, 2),
                new TradeCard(0, 0, 3, 0, 0, 0, 0, 3),
                new TradeCard(0, 0, 0, 1, 2, 2, 0, 0),
                new TradeCard(0, 0, 0, 1, 0, 3, 0, 0),
                new TradeCard(0, 0, 0, 1, 3, 0, 1, 0),
                new TradeCard(0, 0, 0, 1, 1, 1, 1, 0),
                new TradeCard(0, 0, 0, 1, 0, 0, 2, 0),
                new TradeCard(0, 0, 0, 2, 0, 3, 2, 0),
                new TradeCard(0, 0, 0, 2, 1, 1, 3, 0));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="players">The number of players.</param>
        public GameState(int players)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableList();
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Play;
            this.Inventory = Enumerable.Range(0, players).ToImmutableDictionary(i => this.Players[i], i => new Inventory(InitialSpices[i], InitialHand));
            this.Tokens = EnumCollection<Token>.Empty.Add(Token.Gold, players * 2).Add(Token.Silver, players * 2);
            this.MerchantCardDeck = InitialMerchantCards.Deal(6, out ImmutableList<MerchantCard> dealtMerchantCards);
            this.MerchantCardTrack = dealtMerchantCards.Select(c => new MerchantStall(c, EnumCollection<Spice>.Empty)).ToImmutableList();
            this.PointCardDeck = InitialPointCards.Deal(5, out ImmutableList<PointCard> dealtPointCards);
            this.PointCardTrack = dealtPointCards;
        }

        private GameState(
            ImmutableList<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            int merchantCardIndexAfforded,
            int upgradesRemaining,
            ImmutableDictionary<PlayerToken, Inventory> inventory,
            EnumCollection<Token> tokens,
            ImmutableList<MerchantCard> merchantCardDeck,
            ImmutableList<MerchantStall> merchantCardTrack,
            ImmutableList<PointCard> pointCardDeck,
            ImmutableList<PointCard> pointCardTrack)
        {
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.MerchantCardIndexAfforded = merchantCardIndexAfforded;
            this.UpgradesRemaining = upgradesRemaining;
            this.Inventory = inventory;
            this.Tokens = tokens;
            this.MerchantCardDeck = merchantCardDeck;
            this.MerchantCardTrack = merchantCardTrack;
            this.PointCardDeck = pointCardDeck;
            this.PointCardTrack = pointCardTrack;
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
        /// Gets the deck of Merchant cards.
        /// </summary>
        public ImmutableList<MerchantCard> MerchantCardDeck { get; }

        /// <summary>
        /// Gets the index of the Merchant card the player can currently acquire.
        /// </summary>
        public int MerchantCardIndexAfforded { get; }

        /// <summary>
        /// Gets the visible track of Merchant cards.
        /// </summary>
        public ImmutableList<MerchantStall> MerchantCardTrack { get; }

        /// <summary>
        /// Gets the phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the deck of Point cards.
        /// </summary>
        public ImmutableList<PointCard> PointCardDeck { get; }

        /// <summary>
        /// Gets the visible track of Point cards.
        /// </summary>
        public ImmutableList<PointCard> PointCardTrack { get; }

        /// <summary>
        /// Gets the available tokens.
        /// </summary>
        public EnumCollection<Token> Tokens { get; }

        /// <summary>
        /// Gets the number of upgrades remaining.
        /// </summary>
        public int UpgradesRemaining { get; }

        /// <inheritdoc />
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

            int comp;

            if ((comp = this.Phase.CompareTo(state.Phase)) != 0 ||
                (comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
                (comp = this.MerchantCardIndexAfforded.CompareTo(state.MerchantCardIndexAfforded)) != 0 ||
                (comp = this.UpgradesRemaining.CompareTo(state.UpgradesRemaining)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0)
            {
                return comp;
            }

            if (this.Inventory != state.Inventory)
            {
                for (var i = 0; i < this.Players.Count; i++)
                {
                    var player = this.Players[i];
                    if ((comp = this.Inventory[player].CompareTo(state.Inventory[player])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if ((comp = CompareUtilities.CompareLists(this.MerchantCardTrack, state.MerchantCardTrack)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.PointCardTrack, state.PointCardTrack)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = ImmutableList.CreateBuilder<Move>();

            switch (this.Phase)
            {
                case Phase.Play:
                    moves.AddRange(Moves.AcquireMove.GenerateMoves(this));
                    moves.AddRange(Moves.PayMove.GenerateMoves(this));
                    moves.AddRange(Moves.ClaimMove.GenerateMoves(this));
                    moves.AddRange(Moves.SpiceCardMove.GenerateMoves(this));
                    moves.AddRange(Moves.UpgradeCardMove.GenerateMoves(this));
                    moves.AddRange(Moves.TradeCardMove.GenerateMoves(this));
                    moves.AddRange(Moves.RestMove.GenerateMoves(this));
                    break;

                case Phase.Acquire:
                    moves.AddRange(Moves.AcquireMove.GenerateMoves(this));
                    moves.AddRange(Moves.PayMove.GenerateMoves(this));
                    break;

                case Phase.Upgrade:
                    moves.AddRange(Moves.UpgradeMove.GenerateMoves(this));
                    moves.AddRange(Moves.YieldUpgradeMove.GenerateMoves(this));
                    break;

                case Phase.Discard:
                    moves.AddRange(Moves.DiscardSpicesMove.GenerateMoves(this));
                    break;

                case Phase.End:
                    break;
            }

            return moves.ToImmutable();
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            if (move.IsDeterministic)
            {
                yield return Weighted.Create(this.MakeMove(move), 1);
                yield break;
            }

            foreach (var outcome in move.GetOutcomes(this))
            {
                yield return outcome;
            }
        }

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="player">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken player)
        {
            var inventory = this.Inventory[player];
            return inventory.PointCards.Sum(c => c.Points) + inventory.Tokens[Token.Silver] + inventory.Tokens[Token.Gold] * 3 + inventory.Caravan.Count - inventory.Caravan[Spice.Turmeric];
        }

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            yield return this;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            return this.Players
                .GroupBy(p => this.GetScore(p))
                .OrderByDescending(g => g.Key)
                .First()
                .ToImmutableList();
        }

        /// <inheritdoc />
        IGameState<Move> IGameState<Move>.MakeMove(Move move) => this.MakeMove(move);

        /// <summary>
        /// Applies the move to the current game state.
        /// </summary>
        /// <param name="move">The <see cref="Move"/> to apply.</param>
        /// <returns>The updated <see cref="GameState"/>.</returns>
        public GameState MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (this.CompareTo(move.State) != 0)
            {
                throw new InvalidOperationException();
            }

            return move.Apply(this);
        }

        internal GameState With(
            ImmutableList<PlayerToken> players = null,
            PlayerToken activePlayer = null,
            Phase? phase = null,
            int? merchantCardIndexAfforded = null,
            int? upgradesRemaining = null,
            ImmutableDictionary<PlayerToken, Inventory> inventory = null,
            EnumCollection<Token> tokens = null,
            ImmutableList<MerchantCard> merchantCardDeck = null,
            ImmutableList<MerchantStall> merchantCardTrack = null,
            ImmutableList<PointCard> pointCardDeck = null,
            ImmutableList<PointCard> pointCardTrack = null)
        {
            return new GameState(
                players ?? this.Players,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                merchantCardIndexAfforded ?? this.MerchantCardIndexAfforded,
                upgradesRemaining ?? this.UpgradesRemaining,
                inventory ?? this.Inventory,
                tokens ?? this.Tokens,
                merchantCardDeck ?? this.MerchantCardDeck,
                merchantCardTrack ?? this.MerchantCardTrack,
                pointCardDeck ?? this.PointCardDeck,
                pointCardTrack ?? this.PointCardTrack);
        }
    }
}
