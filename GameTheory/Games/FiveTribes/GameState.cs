﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using GameTheory.Games.FiveTribes.Djinns;
    using GameTheory.Games.FiveTribes.Moves;
    using GameTheory.Games.FiveTribes.Tiles;

    /// <summary>
    /// Represents the current state of a game of Five Tribes.
    /// </summary>
    public class GameState : IGameState<Move>
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
        /// The number of tribes in the game.
        /// </summary>
        public const int TribesCount = 5;

        /// <summary>
        /// The initial list of <see cref="Tile">Tiles</see>.
        /// </summary>
        public static readonly ImmutableList<Tile> InitialTiles;

        /// <summary>
        /// The value, in Gold Coins (GC), of <see cref="Resource"/> suits of various sizes.
        /// </summary>
        public static readonly ImmutableList<int> SuitValues;

        /// <summary>
        /// The cost, in Gold Coins (GC), of bidding on the spots in the Turn Order Track.
        /// </summary>
        public static readonly ImmutableList<int> TurnOrderTrackCosts;

        private static readonly ImmutableList<Djinn> Djinns;
        private static readonly EnumCollection<Meeple> Meeples;
        private static readonly EnumCollection<Resource> Resources;
        private readonly ImmutableDictionary<string, string> additionalState;
        private readonly Lazy<ImmutableList<Move>> subsequentMoves;
        private readonly Func<GameState, IEnumerable<Move>> subsequentMovesFactory;

        static GameState()
        {
            TurnOrderTrackCosts = ImmutableList.Create(0, 0, 0, 1, 3, 5, 8, 12, 18);

            SuitValues = ImmutableList.Create(0, 1, 3, 7, 13, 21, 30, 40, 50, 60);

            var smallSacredPlace = new SacredPlace(6);
            InitialTiles = ImmutableList.Create<Tile>(
                BigMarket.Instance,
                BigMarket.Instance,
                BigMarket.Instance,
                BigMarket.Instance,
                SmallMarket.Instance,
                SmallMarket.Instance,
                SmallMarket.Instance,
                SmallMarket.Instance,
                SmallMarket.Instance,
                SmallMarket.Instance,
                SmallMarket.Instance,
                SmallMarket.Instance,
                Oasis.Instance,
                Oasis.Instance,
                Oasis.Instance,
                Oasis.Instance,
                Oasis.Instance,
                Oasis.Instance,
                Village.Instance,
                Village.Instance,
                Village.Instance,
                Village.Instance,
                Village.Instance,
                smallSacredPlace,
                smallSacredPlace,
                smallSacredPlace,
                smallSacredPlace,
                new SacredPlace(10),
                new SacredPlace(12),
                new SacredPlace(15));

            Meeples = new EnumCollection<Meeple>(new[]
            {
                Enumerable.Range(0, 16).Select(_ => Meeple.Vizier),
                Enumerable.Range(0, 20).Select(_ => Meeple.Elder),
                Enumerable.Range(0, 18).Select(_ => Meeple.Builder),
                Enumerable.Range(0, 18).Select(_ => Meeple.Merchant),
                Enumerable.Range(0, 18).Select(_ => Meeple.Assassin),
            }.SelectMany(x => x));

            Djinns = ImmutableList.Create<Djinn>(
                AlAmin.Instance,
                AnunNak.Instance,
                Baal.Instance,
                Boaz.Instance,
                Bouraq.Instance,
                Echidna.Instance,
                Enki.Instance,
                Hagis.Instance,
                Haurvatat.Instance,
                Ibus.Instance,
                Jafaar.Instance,
                Kandicha.Instance,
                Kumarbi.Instance,
                Lamia.Instance,
                Leta.Instance,
                Marid.Instance,
                Monkir.Instance,
                Nekir.Instance,
                Shamhat.Instance,
                Sibittis.Instance,
                Sloar.Instance,
                Utug.Instance);

            Resources = new EnumCollection<Resource>(new[]
            {
                Enumerable.Range(0, 2).Select(_ => Resource.Ivory),
                Enumerable.Range(0, 2).Select(_ => Resource.Jewels),
                Enumerable.Range(0, 2).Select(_ => Resource.Gold),
                Enumerable.Range(0, 4).Select(_ => Resource.Papyrus),
                Enumerable.Range(0, 4).Select(_ => Resource.Silk),
                Enumerable.Range(0, 4).Select(_ => Resource.Spice),
                Enumerable.Range(0, 6).Select(_ => Resource.Fish),
                Enumerable.Range(0, 6).Select(_ => Resource.Wheat),
                Enumerable.Range(0, 6).Select(_ => Resource.Pottery),
                Enumerable.Range(0, 18).Select(_ => Resource.Slave),
            }.SelectMany(x => x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="players">The number of players.</param>
        /// <param name="includeDhenim">Include the promo card, Dhenim, with the Djins?</param>
        public GameState(int players, bool includeDhenim = false)
            : this(null)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableList();
            this.Phase = Phase.Bid;
            this.BidOrderTrack = ImmutableQueue.CreateRange((players == 2 ? this.Players.Concat(this.Players) : this.Players).Shuffle());
            this.TurnOrderTrack = ImmutableList.CreateRange(new PlayerToken[TurnOrderTrackCosts.Count]);
            this.Inventory = this.Players.ToImmutableDictionary(p => p, p => new Inventory());
            this.AssassinationTables = this.Players.ToImmutableDictionary(p => p, p => new AssassinationTable());
            this.ScoreTables = this.Players.ToImmutableDictionary(p => p, p => new ScoreTable());
            this.Sultanate = ImmutableList.CreateRange(InitialTiles.Shuffle().Zip(Meeples.Shuffle().Partition(3), (t, ms) => new Square(t, new EnumCollection<Meeple>(ms))));
            this.Bag = EnumCollection<Meeple>.Empty;
            this.DjinnPile = (includeDhenim ? GameState.Djinns.Add(Dhenim.Instance) : GameState.Djinns).Deal(3, out ImmutableList<Djinn> visibleDjinns);
            this.VisibleDjinns = visibleDjinns;
            this.DjinnDiscards = ImmutableList<Djinn>.Empty;
            this.ResourcePile = GameState.Resources.Deal(9, out ImmutableList<Resource> visibleResources);
            this.VisibleResources = visibleResources;
            this.ResourceDiscards = EnumCollection<Resource>.Empty;
            this.additionalState = ImmutableDictionary<string, string>.Empty;
        }

        internal GameState(
            ImmutableDictionary<string, string> additionalState,
            ImmutableDictionary<PlayerToken, AssassinationTable> assassinationTables,
            EnumCollection<Meeple> bag,
            ImmutableQueue<PlayerToken> bidOrderTrack,
            ImmutableList<Djinn> djinnDiscards,
            ImmutableList<Djinn> djinnPile,
            EnumCollection<Meeple> inHand,
            ImmutableDictionary<PlayerToken, Inventory> inventory,
            Point lastPoint,
            Phase phase,
            ImmutableList<PlayerToken> players,
            Point previousPoint,
            EnumCollection<Resource> resourceDiscards,
            EnumCollection<Resource> resourcePile,
            ImmutableDictionary<PlayerToken, ScoreTable> scoreTables,
            Func<GameState, IEnumerable<Move>> subsequentMovesFactory,
            ImmutableList<Square> sultanate,
            ImmutableList<PlayerToken> turnOrderTrack,
            ImmutableList<Djinn> visibleDjinns,
            ImmutableList<Resource> visibleResources)
            : this(subsequentMovesFactory)
        {
            this.additionalState = additionalState;
            this.AssassinationTables = assassinationTables;
            this.Bag = bag;
            this.BidOrderTrack = bidOrderTrack;
            this.DjinnDiscards = djinnDiscards;
            this.DjinnPile = djinnPile;
            this.InHand = inHand;
            this.Inventory = inventory;
            this.LastPoint = lastPoint;
            this.Phase = phase;
            this.Players = players;
            this.PreviousPoint = previousPoint;
            this.ResourceDiscards = resourceDiscards;
            this.ResourcePile = resourcePile;
            this.ScoreTables = scoreTables;
            this.Sultanate = sultanate;
            this.TurnOrderTrack = turnOrderTrack;
            this.VisibleDjinns = visibleDjinns;
            this.VisibleResources = visibleResources;
        }

        private GameState(Func<GameState, IEnumerable<Move>> subsequentMovesFactory)
        {
            this.subsequentMovesFactory = subsequentMovesFactory;
            this.subsequentMoves = subsequentMovesFactory == null ? null : new Lazy<ImmutableList<Move>>(() => this.subsequentMovesFactory(this).ToImmutableList(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Gets the currently active player.
        /// </summary>
        public PlayerToken ActivePlayer =>
            this.Phase == Phase.End ? null :
            this.Phase == Phase.Bid ? this.BidOrderTrack.Peek() :
            this.Phase == Phase.MoveTurnMarker ? this.TurnOrderTrack[this.FindHighestBidIndex()] :
            this.BidOrderTrack.Last();

        /// <summary>
        /// Gets the <see cref="AssassinationTable">AssassinationTables</see> for all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, AssassinationTable> AssassinationTables { get; }

        /// <summary>
        /// Gets the Bag of <see cref="Meeple">Meeples</see>.
        /// </summary>
        public EnumCollection<Meeple> Bag { get; }

        /// <summary>
        /// Gets the Bid Order Track.
        /// </summary>
        public ImmutableQueue<PlayerToken> BidOrderTrack { get; }

        /// <summary>
        /// Gets the per-player Camel limit.
        /// </summary>
        public int CamelLimit => this.Players.Count > 2 ? 8 : 11;

        /// <summary>
        /// Gets the <see cref="Djinn"/> discard pile.
        /// </summary>
        public ImmutableList<Djinn> DjinnDiscards { get; }

        /// <summary>
        /// Gets the <see cref="Djinn"/> draw pile.
        /// </summary>
        public ImmutableList<Djinn> DjinnPile { get; }

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="GameState"/> contains subsequent moves.
        /// </summary>
        public bool HasSubsequentMoves =>
            this.subsequentMovesFactory != null && !this.subsequentMoves.Value.IsEmpty;

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> in the active player's hand.
        /// </summary>
        public EnumCollection<Meeple> InHand { get; }

        /// <summary>
        /// Gets the <see cref="Inventory">Inventories</see> for all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, Inventory> Inventory { get; }

        /// <summary>
        /// Gets the last <see cref="Point"/> in the <see cref="Sultanate"/> that had <see cref="Meeple">Meeples</see> picked up or dropped.
        /// </summary>
        public Point LastPoint { get; }

        /// <summary>
        /// Gets the current <see cref="Phase"/>.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players { get; }

        /// <summary>
        /// Gets the previous-to-last <see cref="Point"/> in the <see cref="Sultanate"/> that had <see cref="Meeple">Meeples</see> picked up or dropped.
        /// </summary>
        public Point PreviousPoint { get; }

        /// <summary>
        /// Gets the <see cref="Resource"/> discard pile.
        /// </summary>
        public EnumCollection<Resource> ResourceDiscards { get; }

        /// <summary>
        /// Gets the <see cref="Resource"/> draw pile.
        /// </summary>
        public EnumCollection<Resource> ResourcePile { get; }

        /// <summary>
        /// Gets the <see cref="ScoreTable">ScoreTables</see> for all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, ScoreTable> ScoreTables { get; }

        /// <summary>
        /// Gets the <see cref="Sultanate"/>.
        /// </summary>
        public ImmutableList<Square> Sultanate { get; }

        /// <summary>
        /// Gets the Turn Order Track.
        /// </summary>
        public ImmutableList<PlayerToken> TurnOrderTrack { get; }

        /// <summary>
        /// Gets the face-up <see cref="Djinn">Djinns</see>.
        /// </summary>
        public ImmutableList<Djinn> VisibleDjinns { get; }

        /// <summary>
        /// Gets the face-up <see cref="Resource">Resources</see>.
        /// </summary>
        public ImmutableList<Resource> VisibleResources { get; }

        /// <summary>
        /// Gets the state value of the specified key.
        /// </summary>
        /// <param name="key">The key of the state value.</param>
        /// <returns>The requested state value.</returns>
        public string this[string key]
        {
            get
            {
                this.additionalState.TryGetValue(key, out string result);
                return result;
            }
        }

        /// <summary>
        /// Gets the value, in Victory Points (VP) or Gold Coins (GC), of a set of <see cref="Resource">Resources</see>.
        /// </summary>
        /// <param name="resources">The resources to score.</param>
        /// <returns>The value of the <see cref="Resource">Resources</see>.</returns>
        public static int ScoreResources(EnumCollection<Resource> resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            var suits = new List<int>();

            foreach (var key in resources.Keys)
            {
                if (key == Resource.Slave)
                {
                    continue;
                }

                var count = resources[key];
                while (suits.Count < count)
                {
                    suits.Add(0);
                }

                for (int i = 0; i < count; i++)
                {
                    suits[i]++;
                }
            }

            return suits.Sum(s => SuitValues[s]);
        }

        /// <summary>
        /// Finds the index of the highest non-null value in the <see cref="TurnOrderTrack"/>.
        /// </summary>
        /// <returns>The requested index.</returns>
        public int FindHighestBidIndex()
        {
            var nextIndex = this.TurnOrderTrack.Count - 1;
            while (nextIndex >= 0 && this.TurnOrderTrack[nextIndex] == null)
            {
                nextIndex--;
            }

            return nextIndex;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves()
        {
            var moves = new List<Move>();

            if (this.subsequentMovesFactory != null)
            {
                moves.AddRange(this.subsequentMoves.Value);
            }
            else
            {
                switch (this.Phase)
                {
                    case Phase.Bid:
                        moves.AddRange(this.GetBidMoves());
                        break;

                    case Phase.MoveTurnMarker:
                        moves.AddRange(this.GetMoveTurnMarkerMoves());
                        break;

                    case Phase.PickUpMeeples:
                        moves.AddRange(this.GetPickUpMeeplesMoves());
                        break;

                    case Phase.MoveMeeples:
                        moves.AddRange(this.GetMoveMeeplesMoves());
                        break;

                    case Phase.TileControlCheck:
                        moves.AddRange(this.GetTileControlCheckMoves());
                        break;

                    case Phase.TribesAction:
                        moves.AddRange(this.GetTribesActionMoves());
                        break;

                    case Phase.TileAction:
                        moves.AddRange(this.GetTileActionMoves());
                        break;

                    case Phase.MerchandiseSale:
                        moves.AddRange(this.GetMerchandiseSaleMoves());
                        break;
                }

                foreach (var playerToken in this.Players)
                {
                    foreach (var djinn in this.Inventory[playerToken].Djinns)
                    {
                        moves.AddRange(djinn.GetMoves(this));
                    }
                }
            }

            var originalMoves = moves.ToImmutableList();
            foreach (var playerToken in this.Players)
            {
                foreach (var djinn in this.Inventory[playerToken].Djinns)
                {
                    moves.AddRange(djinn.GetAdditionalMoves(this, originalMoves));
                }
            }

            return moves.ToImmutableList();
        }

        /// <summary>
        /// Gets the score, in Victory Points (VP), of the specified player.
        /// </summary>
        /// <param name="playerToken">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken playerToken)
        {
            var inventory = this.Inventory[playerToken];
            var scoreTable = this.ScoreTables[playerToken];
            var owned = this.Sultanate.Where(b => b.Owner == playerToken).ToList();
            var viziers = inventory.Meeples.Count(m => m == Meeple.Vizier);
            var playersWithFewerViziers = this.Inventory.Values.Count(i => i.Meeples.Count(m => m == Meeple.Vizier) < viziers);
            return inventory.GoldCoins +
                   ScoreResources(inventory.Resources) +
                   viziers * scoreTable.VizierValue +
                   playersWithFewerViziers * 10 +
                   inventory.Meeples.Count(m => m == Meeple.Elder) * scoreTable.ElderValue +
                   owned.Sum(s => s.Tile.Value + s.Palaces * scoreTable.PalaceValue + s.PalmTrees * scoreTable.PalmTreeValue) +
                   inventory.Djinns.Sum(d => d.Value);
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
        IGameState<Move> IGameState<Move>.MakeMove(Move move)
        {
            return this.MakeMove(move);
        }

        /// <inheritdoc />
        public IGameState<Move> GetView(PlayerToken playerToken) => this;

        /// <summary>
        /// Gets a value indicating whether or not the specified player has any Camels left.
        /// </summary>
        /// <param name="playerToken">The player whose camel count should be checked against the <see cref="CamelLimit"/>.</param>
        /// <returns><c>true</c> if the player has any remaining camels, <c>false</c> otherwise.</returns>
        public bool IsPlayerUnderCamelLimit(PlayerToken playerToken)
        {
            return this.Sultanate.Count(s => s.Owner == playerToken) < this.CamelLimit;
        }

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

            if (move.State != this)
            {
                throw new InvalidOperationException();
            }

            var newState = move.Apply(this);
            return HandleTransition(this, newState);
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            if (move.IsDeterministic)
            {
                yield return Weighted.Create(this.MakeMove(move), 1);
                yield break;
            }

            // BUG: Need to enumerate possible outcomes and combine scores with expected occurence probabilities.
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <summary>
        /// Creates a new <see cref="GameState"/>, and updates the specified values.
        /// </summary>
        /// <param name="assassinationTables"><c>null</c> to keep the existing value, or any other value to update <see cref="AssassinationTables"/>.</param>
        /// <param name="bag"><c>null</c> to keep the existing value, or any other value to update <see cref="Bag"/>.</param>
        /// <param name="bidOrderTrack"><c>null</c> to keep the existing value, or any other value to update <see cref="BidOrderTrack"/>.</param>
        /// <param name="djinnDiscards"><c>null</c> to keep the existing value, or any other value to update <see cref="DjinnDiscards"/>.</param>
        /// <param name="djinnPile"><c>null</c> to keep the existing value, or any other value to update <see cref="DjinnPile"/>.</param>
        /// <param name="inHand"><c>null</c> to keep the existing value, or any other value to update <see cref="InHand"/>.</param>
        /// <param name="inventory"><c>null</c> to keep the existing value, or any other value to update <see cref="Inventory"/>.</param>
        /// <param name="lastPoint"><c>null</c> to keep the existing value, or any other value to update <see cref="LastPoint"/>.</param>
        /// <param name="phase"><c>null</c> to keep the existing value, or any other value to update <see cref="Phase"/>.</param>
        /// <param name="players"><c>null</c> to keep the existing value, or any other value to update <see cref="Players"/>.</param>
        /// <param name="previousPoint"><c>null</c> to keep the existing value, or any other value to update <see cref="PreviousPoint"/>.</param>
        /// <param name="resourceDiscards"><c>null</c> to keep the existing value, or any other value to update <see cref="ResourceDiscards"/>.</param>
        /// <param name="resourcePile"><c>null</c> to keep the existing value, or any other value to update <see cref="ResourcePile"/>.</param>
        /// <param name="scoreTables"><c>null</c> to keep the existing value, or any other value to update <see cref="ScoreTables"/>.</param>
        /// <param name="sultanate"><c>null</c> to keep the existing value, or any other value to update <see cref="Sultanate"/>.</param>
        /// <param name="turnOrderTrack"><c>null</c> to keep the existing value, or any other value to update <see cref="TurnOrderTrack"/>.</param>
        /// <param name="visibleDjinns"><c>null</c> to keep the existing value, or any other value to update <see cref="VisibleDjinns"/>.</param>
        /// <param name="visibleResources"><c>null</c> to keep the existing value, or any other value to update <see cref="VisibleResources"/>.</param>
        /// <returns>The new <see cref="GameState"/>.</returns>
        public GameState With(ImmutableDictionary<PlayerToken, AssassinationTable> assassinationTables = null, EnumCollection<Meeple> bag = null, ImmutableQueue<PlayerToken> bidOrderTrack = null, ImmutableList<Djinn> djinnDiscards = null, ImmutableList<Djinn> djinnPile = null, EnumCollection<Meeple> inHand = null, ImmutableDictionary<PlayerToken, Inventory> inventory = null, Point? lastPoint = null, Phase? phase = null, ImmutableList<PlayerToken> players = null, Point? previousPoint = null, EnumCollection<Resource> resourceDiscards = null, EnumCollection<Resource> resourcePile = null, ImmutableDictionary<PlayerToken, ScoreTable> scoreTables = null, ImmutableList<Square> sultanate = null, ImmutableList<PlayerToken> turnOrderTrack = null, ImmutableList<Djinn> visibleDjinns = null, ImmutableList<Resource> visibleResources = null)
        {
            return new GameState(
                this.additionalState,
                assassinationTables ?? this.AssassinationTables,
                bag ?? this.Bag,
                bidOrderTrack ?? this.BidOrderTrack,
                djinnDiscards ?? this.DjinnDiscards,
                djinnPile ?? this.DjinnPile,
                inHand ?? this.InHand,
                inventory ?? this.Inventory,
                lastPoint ?? this.LastPoint,
                phase ?? this.Phase,
                players ?? this.Players,
                previousPoint ?? this.PreviousPoint,
                resourceDiscards ?? this.ResourceDiscards,
                resourcePile ?? this.ResourcePile,
                scoreTables ?? this.ScoreTables,
                null,
                sultanate ?? this.Sultanate,
                turnOrderTrack ?? this.TurnOrderTrack,
                visibleDjinns ?? this.VisibleDjinns,
                visibleResources ?? this.VisibleResources);
        }

        /// <summary>
        /// Creates a new <see cref="GameState"/> with the specified subsequent <see cref="Move">Moves</see>.
        /// </summary>
        /// <param name="subsequentMoves">A function that generates subsequent <see cref="Move">Moves</see>.</param>
        /// <returns>The new <see cref="GameState"/>.</returns>
        public GameState WithMoves(Func<GameState, Move> subsequentMoves)
        {
            return this.WithMoves(s => new[] { subsequentMoves(s) });
        }

        /// <summary>
        /// Creates a new <see cref="GameState"/> with the specified subsequent <see cref="Move">Moves</see>.
        /// </summary>
        /// <param name="subsequentMoves">A function that generates subsequent <see cref="Move">Moves</see>.</param>
        /// <returns>The new <see cref="GameState"/>.</returns>
        public GameState WithMoves(Func<GameState, IEnumerable<Move>> subsequentMoves)
        {
            return new GameState(
                this.additionalState,
                this.AssassinationTables,
                this.Bag,
                this.BidOrderTrack,
                this.DjinnDiscards,
                this.DjinnPile,
                this.InHand,
                this.Inventory,
                this.LastPoint,
                this.Phase,
                this.Players,
                this.PreviousPoint,
                this.ResourceDiscards,
                this.ResourcePile,
                this.ScoreTables,
                subsequentMoves,
                this.Sultanate,
                this.TurnOrderTrack,
                this.VisibleDjinns,
                this.VisibleResources);
        }

        /// <summary>
        /// Creates a new <see cref="GameState"/>, and updates the specified state value.
        /// </summary>
        /// <param name="key">The state key.</param>
        /// <param name="value">The state value.</param>
        /// <returns>The new <see cref="GameState"/>.</returns>
        public GameState WithState(string key, string value)
        {
            return new GameState(
                value == null ? this.additionalState.Remove(key) : this.additionalState.SetItem(key, value),
                this.AssassinationTables,
                this.Bag,
                this.BidOrderTrack,
                this.DjinnDiscards,
                this.DjinnPile,
                this.InHand,
                this.Inventory,
                this.LastPoint,
                this.Phase,
                this.Players,
                this.PreviousPoint,
                this.ResourceDiscards,
                this.ResourcePile,
                this.ScoreTables,
                null,
                this.Sultanate,
                this.TurnOrderTrack,
                this.VisibleDjinns,
                this.VisibleResources);
        }

        private static IEnumerable<Move> GetAssassinationMoves(GameState state, int slaves = 0)
        {
            var player0 = state.ActivePlayer;

            Func<EnumCollection<Meeple>, IEnumerable<EnumCollection<Meeple>>> combos = killable =>
            {
                return killable.Combinations(Math.Min(killable.Count, state.AssassinationTables[player0].KillCount));
            };

            foreach (var victim in state.Players.Except(player0))
            {
                if (state.AssassinationTables[victim].HasProtection)
                {
                    continue;
                }

                foreach (var kill in combos(state.Inventory[victim].Meeples))
                {
                    yield return new AssassinatePlayerMove(state, victim, kill, s => s.With(phase: FiveTribes.Phase.TileAction));
                }
            }

            foreach (var point in FiveTribes.Sultanate.GetPointsWithin(state.LastPoint, state.InHand.Count + slaves))
            {
                foreach (var kill in combos(state.Sultanate[point].Meeples))
                {
                    yield return new AssassinateMove(state, point, kill, s => s.With(phase: FiveTribes.Phase.TileAction));
                }
            }
        }

        private static GameState HandleTransition(GameState oldState, GameState newState)
        {
            if (newState.subsequentMovesFactory != null && newState.subsequentMoves.IsValueCreated)
            {
                throw new InvalidOperationException();
            }

            var oldestStates = Djinns.ToImmutableDictionary(d => d, d => oldState);

            var subsequentMoves = newState.subsequentMovesFactory;

            var changedEver = false;
            while (true)
            {
                var changed = false;

                foreach (var player in newState.Players)
                {
                    foreach (var djinn in newState.Inventory[player].Djinns)
                    {
                        var oldestState = oldestStates[djinn];
                        if (oldestState != newState)
                        {
                            var nextState = djinn.HandleTransition(player, oldestState, newState);
                            oldestStates = oldestStates.SetItem(djinn, nextState);
                            if (nextState != newState)
                            {
                                newState = nextState;
                                changedEver = changed = true;
                            }
                        }
                    }
                }

                if (!changed)
                {
                    break;
                }
            }

            if (changedEver && subsequentMoves != null)
            {
                newState = newState.WithMoves(subsequentMoves);
            }

            return newState;
        }

        private IEnumerable<Move> GetBidMoves()
        {
            if (this.Phase != FiveTribes.Phase.Bid)
            {
                throw new InvalidOperationException();
            }

            for (var i = 2; i < this.TurnOrderTrack.Count; i++)
            {
                if (this.TurnOrderTrack[i] == null && this.Inventory[this.ActivePlayer].GoldCoins >= TurnOrderTrackCosts[i])
                {
                    var j = i == 2 && this.TurnOrderTrack[0] == null ? 0 :
                            i == 2 && this.TurnOrderTrack[1] == null ? 1 :
                            i;

                    yield return new BidMove(this, j, TurnOrderTrackCosts[j]);
                }
            }
        }

        private IEnumerable<Move> GetMerchandiseSaleMoves()
        {
            yield return new EndTurnMove(this);

            var keys = this.Inventory[this.ActivePlayer].Resources.Keys.ToImmutableList().RemoveAll(r => r == Resource.Slave);

            for (var i = (1 << keys.Count) - 1; i > 0; i--)
            {
                var resources = new EnumCollection<Resource>(keys.Select((k, j) => new { k, j }).Where(x => (i & 1 << x.j) != 0).Select(x => x.k));

                yield return new SellMerchandiseMove(this, resources);
            }
        }

        private IEnumerable<Move> GetMoveMeeplesMoves()
        {
            var drops = this.Sultanate.GetMoves(this.LastPoint, this.PreviousPoint, this.InHand);
            foreach (var drop in drops)
            {
                var meeple = drop.Item1;
                var point = drop.Item2;

                yield return new DropMeepleMove(this, meeple, point);
            }
        }

        private IEnumerable<Move> GetMoveTurnMarkerMoves()
        {
            yield return new MoveTurnMarkerMove(this);
        }

        private IEnumerable<Move> GetPickUpMeeplesMoves()
        {
            var any = false;
            foreach (var point in this.Sultanate.GetPickUps())
            {
                any = true;
                yield return new PickUpMeeplesMove(this, point);
            }

            if (!any)
            {
                yield return new ChangePhaseMove(this, "Skip move", FiveTribes.Phase.MerchandiseSale);
            }
        }

        private IEnumerable<Move> GetTileActionMoves()
        {
            return this.Sultanate[this.LastPoint].Tile.GetTileActionMoves(this);
        }

        private IEnumerable<Move> GetTileControlCheckMoves()
        {
            yield return new PlaceCamelMove(this, this.LastPoint, s => s.With(phase: FiveTribes.Phase.TribesAction));
        }

        private IEnumerable<Move> GetTribesActionMoves()
        {
            var activePlayer = this.ActivePlayer;
            var tribe = this.InHand.Keys.Single();
            switch (tribe)
            {
                case Meeple.Vizier:
                case Meeple.Elder:
                    yield return new TakeMeeplesInHandMove(this);
                    break;

                case Meeple.Merchant:
                    yield return new TradeMerchantsInHandMove(this);
                    break;

                case Meeple.Builder:

                    yield return new ScoreBuildersInHandMove(this, 0);

                    var moves = Cost.OneOrMoreSlaves(this, s => s, (s1, slaves) => new[]
                    {
                        new ScoreBuildersInHandMove(s1, slaves),
                    });

                    break;

                case Meeple.Assassin:
                    var any = false;

                    foreach (var m in GetAssassinationMoves(this))
                    {
                        any = true;
                        yield return m;
                    }

                    if (!any)
                    {
                        yield return new ChangePhaseMove(this, "Skip Tribes Action", FiveTribes.Phase.TileAction);
                    }

                    var costMoves = Cost.OneOrMoreSlaves(this, s => s, GetAssassinationMoves);

                    foreach (var m in costMoves)
                    {
                        yield return m;
                    }

                    break;
            }
        }
    }
}
