// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using GameTheory.Games.LoveLetter.Moves;
    using GameTheory.Games.LoveLetter.States;

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
        /// The number of tokens required to win.
        /// </summary>
        public static readonly ImmutableDictionary<int, int> WinThresholds = ImmutableDictionary.Create<int, int>()
            .Add(2, 7)
            .Add(3, 5)
            .Add(4, 4);

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

        private InterstitialState interstitialState;

        private Lazy<ImmutableList<Move>> subsequentMoves;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        /// <param name="players">The number of players.</param>
        public GameState(
            [Range(MinPlayers, MaxPlayers)]
            [Display(ResourceType = typeof(SharedResources), Name = nameof(SharedResources.Players), Description = nameof(SharedResources.PlayersDescription))]
            int players = MinPlayers)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(players, MinPlayers);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(players, MaxPlayers);

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableArray();
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Draw;
            var inventory = this.Players.ToImmutableDictionary(p => p, p => new Inventory());
            DealNewRound(ref inventory, out var deck, out var hidden, out var inaccessible);
            this.Inaccessible = inaccessible;
            this.Inventory = inventory;
            this.Hidden = hidden;

            this.Deck = deck;
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            InterstitialState interstitialState,
            Card hidden,
            EnumCollection<Card> inaccessible,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableDictionary<PlayerToken, Inventory> inventory,
            EnumCollection<Card> deck)
            : this(interstitialState)
        {
            this.Players = players;
            this.Hidden = hidden;
            this.Inaccessible = inaccessible;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Inventory = inventory;
            this.Deck = deck;
        }

        private GameState(InterstitialState interstitialState)
        {
            this.interstitialState = interstitialState;
            this.subsequentMoves = this.interstitialState == null ? null : new Lazy<ImmutableList<Move>>(() => this.interstitialState.GenerateMoves(this).ToImmutableList(), LazyThreadSafetyMode.ExecutionAndPublication);
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
        public ImmutableDictionary<PlayerToken, Inventory> Inventory { get; }

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
                (comp = EnumComparer<Phase>.Default.Compare(this.Phase, state.Phase)) != 0 ||
                (comp = this.Hidden.CompareTo(state.Hidden)) != 0 ||
                (comp = this.Inaccessible.CompareTo(state.Inaccessible)) != 0 ||
                (comp = CompareUtilities.CompareDictionaries(this.Inventory, state.Inventory)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0 ||
                (comp = this.Deck.CompareTo(state.Deck)) != 0)
            {
                return comp;
            }

            if (this.interstitialState != state.interstitialState)
            {
                if (this.interstitialState == null)
                {
                    return -1;
                }

                if ((comp = this.interstitialState.CompareTo(state.interstitialState)) != 0)
                {
                    return comp;
                }
            }

            return 0;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => this.CompareTo(obj as IGameState<Move>) == 0;

        /// <inheritdoc />
        public IReadOnlyList<Move> GetAvailableMoves()
        {
            var moves = new List<Move>();

            if (this.interstitialState != null)
            {
                moves.AddRange(this.subsequentMoves.Value);
            }
            else
            {
                switch (this.Phase)
                {
                    case Phase.Draw:
                        moves.AddRange(DrawCardMove.GenerateMoves(this));
                        break;

                    case Phase.Discard:
                        moves.AddRange(DiscardCardMove.GenerateMoves(this));
                        break;

                    case Phase.Reveal:
                        moves.AddRange(RevealHandMove.GenerateMoves(this));
                        break;

                    case Phase.Deal:
                        moves.AddRange(DealMove.GenerateMoves(this));
                        break;
                }
            }

            return moves.ToImmutableList();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.ActivePlayer.GetHashCode());
            HashUtilities.Combine(ref hash, this.Phase.GetHashCode());

            for (var i = 0; i < this.Players.Length; i++)
            {
                HashUtilities.Combine(ref hash, this.Inventory[this.Players[i]].GetHashCode());
            }

            return hash;
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

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            PlayerToken revealedPlayer = null;
            switch (this.interstitialState)
            {
                case ComparingHandsState comparingHands:
                    if (playerToken == this.ActivePlayer)
                    {
                        revealedPlayer = comparingHands.TargetPlayer;
                    }
                    else if (playerToken == comparingHands.TargetPlayer)
                    {
                        revealedPlayer = this.ActivePlayer;
                    }

                    break;

                case LookAtHandState lookAtHand:
                    if (playerToken == this.ActivePlayer)
                    {
                        revealedPlayer = lookAtHand.TargetPlayer;
                    }

                    break;
            }

            var newState = this;
            if (revealedPlayer != null)
            {
                var inventory = this.Inventory;
                newState = newState.With(
                    keepInterstitial: true,
                    inventory: inventory.SetItem(revealedPlayer, inventory[revealedPlayer].With(
                        handRevealed: true)));
            }

            var shuffler = new GameShuffler<GameState>(newState);
            if (this.Hidden != Card.None)
            {
                shuffler.Add(
                    "Cards",
                    this.Hidden,
                    (state, value) => state.With(keepInterstitial: true, hidden: value));
            }

            shuffler.AddCollection(
                "Cards",
                this.Deck,
                (state, value) => state.With(keepInterstitial: true, deck: new EnumCollection<Card>(value)));

            foreach (var p in this.Players)
            {
                var player = p;
                var inventory = newState.Inventory[player];

                if (p == playerToken || inventory.HandRevealed)
                {
                    continue;
                }

                shuffler.AddCollection(
                    "Cards",
                    inventory.Hand,
                    (state, value) => state.With(
                        keepInterstitial: true,
                        inventory: state.Inventory.SetItem(player, state.Inventory[player].With(
                            hand: value.ToImmutableArray()))));
            }

            return shuffler.Take(maxStates);
        }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> GetWinners()
        {
            var winThreshold = WinThresholds[this.Players.Length];
            return this.Players.Where(p => this.Inventory[p].Tokens >= winThreshold).ToList();
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
            ArgumentNullException.ThrowIfNull(move);

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

        internal static void DealNewRound(ref ImmutableDictionary<PlayerToken, Inventory> inventory, out EnumCollection<Card> deck, out Card hidden, out EnumCollection<Card> inaccessible)
        {
            deck = StartingDeck;
            deck = deck.Deal(out hidden);

            if (inventory.Count == MinPlayers)
            {
                deck = deck.Deal(3, out var dealt);
                inaccessible = new EnumCollection<Card>(dealt);
            }
            else
            {
                inaccessible = EnumCollection<Card>.Empty;
            }

            var inventoryBuilder = ImmutableDictionary.CreateBuilder<PlayerToken, Inventory>();
            foreach (var player in inventory.Keys)
            {
                deck = deck.Deal(out var startingHand);
                var playerInventory = new Inventory(
                    hand: ImmutableArray.Create(startingHand),
                    tokens: inventory[player].Tokens,
                    discards: ImmutableStack<Card>.Empty,
                    handRevealed: false);
                inventoryBuilder.Add(
                    player,
                    playerInventory);
            }

            inventory = inventoryBuilder.ToImmutable();
        }

        internal GameState With(
            bool keepInterstitial = false,
            Card? hidden = null,
            EnumCollection<Card> inaccessible = null,
            PlayerToken activePlayer = null,
            Phase? phase = null,
            ImmutableDictionary<PlayerToken, Inventory> inventory = null,
            EnumCollection<Card> deck = null)
        {
            return new GameState(
                this.Players,
                keepInterstitial ? this.interstitialState : null,
                hidden ?? this.Hidden,
                inaccessible ?? this.Inaccessible,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                inventory ?? this.Inventory,
                deck ?? this.Deck);
        }

        internal GameState WithInterstitialState(InterstitialState interstitialState)
        {
            return new GameState(
                this.Players,
                interstitialState,
                this.Hidden,
                this.Inaccessible,
                this.ActivePlayer,
                this.Phase,
                this.Inventory,
                this.Deck);
        }
    }
}
