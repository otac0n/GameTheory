// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        private readonly ImmutableDictionary<PlayerToken, AssassinationTable> assassinationTables;
        private readonly EnumCollection<Meeple> bag;
        private readonly ImmutableQueue<PlayerToken> bidOrderTrack;
        private readonly ImmutableList<Djinn> djinnDiscards;
        private readonly ImmutableList<Djinn> djinnPile;
        private readonly EnumCollection<Meeple> inHand;
        private readonly ImmutableDictionary<PlayerToken, Inventory> inventory;
        private readonly Point lastPoint;
        private readonly Phase phase;
        private readonly ImmutableList<PlayerToken> players;
        private readonly Point previousPoint;
        private readonly EnumCollection<Resource> resourceDiscards;
        private readonly EnumCollection<Resource> resourcePile;
        private readonly ImmutableDictionary<PlayerToken, ScoreTable> scoreTables;
        private readonly Lazy<ImmutableList<Move>> subsequentMoves;
        private readonly Func<GameState, IEnumerable<Move>> subsequentMovesFactory;
        private readonly ImmutableList<Square> sultanate;
        private readonly ImmutableList<PlayerToken> turnOrderTrack;
        private readonly ImmutableList<Djinn> visibleDjinns;
        private readonly ImmutableList<Resource> visibleResources;

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

            this.players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableList();
            this.phase = Phase.Bid;
            this.bidOrderTrack = ImmutableQueue.CreateRange((players == 2 ? this.players.Concat(this.players) : this.players).Shuffle());
            this.turnOrderTrack = ImmutableList.CreateRange(new PlayerToken[TurnOrderTrackCosts.Count]);
            this.inventory = this.players.ToImmutableDictionary(p => p, p => new Inventory());
            this.assassinationTables = this.players.ToImmutableDictionary(p => p, p => new AssassinationTable());
            this.scoreTables = this.players.ToImmutableDictionary(p => p, p => new ScoreTable());
            this.sultanate = ImmutableList.CreateRange(InitialTiles.Shuffle().Zip(Meeples.Shuffle().Partition(3), (t, ms) => new Square(t, new EnumCollection<Meeple>(ms))));
            this.bag = EnumCollection<Meeple>.Empty;
            this.djinnPile = (includeDhenim ? GameState.Djinns.Add(Dhenim.Instance) : GameState.Djinns).Deal(3, out this.visibleDjinns);
            this.djinnDiscards = ImmutableList<Djinn>.Empty;
            this.resourcePile = GameState.Resources.Deal(9, out this.visibleResources);
            this.resourceDiscards = EnumCollection<Resource>.Empty;
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
            this.assassinationTables = assassinationTables;
            this.bag = bag;
            this.bidOrderTrack = bidOrderTrack;
            this.djinnDiscards = djinnDiscards;
            this.djinnPile = djinnPile;
            this.inHand = inHand;
            this.inventory = inventory;
            this.lastPoint = lastPoint;
            this.phase = phase;
            this.players = players;
            this.previousPoint = previousPoint;
            this.resourceDiscards = resourceDiscards;
            this.resourcePile = resourcePile;
            this.scoreTables = scoreTables;
            this.sultanate = sultanate;
            this.turnOrderTrack = turnOrderTrack;
            this.visibleDjinns = visibleDjinns;
            this.visibleResources = visibleResources;
        }

        private GameState(Func<GameState, IEnumerable<Move>> subsequentMovesFactory)
        {
            this.subsequentMovesFactory = subsequentMovesFactory;
            this.subsequentMoves = subsequentMovesFactory == null ? null : new Lazy<ImmutableList<Move>>(() => this.subsequentMovesFactory(this).ToImmutableList(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Gets the currently active player.
        /// </summary>
        public PlayerToken ActivePlayer
        {
            get
            {
                return this.phase == Phase.End ? null :
                       this.phase == Phase.Bid ? this.bidOrderTrack.Peek() :
                       this.phase == Phase.MoveTurnMarker ? this.turnOrderTrack[this.FindHighestBidIndex()] :
                       this.bidOrderTrack.Last();
            }
        }

        /// <summary>
        /// Gets the <see cref="AssassinationTable">AssassinationTables</see> for all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, AssassinationTable> AssassinationTables
        {
            get { return this.assassinationTables; }
        }

        /// <summary>
        /// Gets the Bag of <see cref="Meeple">Meeples</see>.
        /// </summary>
        public EnumCollection<Meeple> Bag
        {
            get { return this.bag; }
        }

        /// <summary>
        /// Gets the Bid Order Track.
        /// </summary>
        public ImmutableQueue<PlayerToken> BidOrderTrack
        {
            get { return this.bidOrderTrack; }
        }

        /// <summary>
        /// Gets the per-player Camel limit.
        /// </summary>
        public int CamelLimit
        {
            get { return this.players.Count > 2 ? 8 : 11; }
        }

        /// <summary>
        /// Gets the <see cref="Djinn"/> discard pile.
        /// </summary>
        public ImmutableList<Djinn> DjinnDiscards
        {
            get { return this.djinnDiscards; }
        }

        /// <summary>
        /// Gets the <see cref="Djinn"/> draw pile.
        /// </summary>
        public ImmutableList<Djinn> DjinnPile
        {
            get { return this.djinnPile; }
        }

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="GameState"/> contains subsequent moves.
        /// </summary>
        public bool HasSubsequentMoves
        {
            get { return this.subsequentMovesFactory != null && !this.subsequentMoves.Value.IsEmpty; }
        }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players
        {
            get { return this.Players; }
        }

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> in the active player's hand.
        /// </summary>
        public EnumCollection<Meeple> InHand
        {
            get { return this.inHand; }
        }

        /// <summary>
        /// Gets the <see cref="Inventory">Inventories</see> for all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, Inventory> Inventory
        {
            get { return this.inventory; }
        }

        /// <summary>
        /// Gets the last <see cref="Point"/> in the <see cref="Sultanate"/> that had <see cref="Meeple">Meeples</see> picked up or dropped.
        /// </summary>
        public Point LastPoint
        {
            get { return this.lastPoint; }
        }

        /// <summary>
        /// Gets the current <see cref="Phase"/>.
        /// </summary>
        public Phase Phase
        {
            get { return this.phase; }
        }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players
        {
            get { return this.players; }
        }

        /// <summary>
        /// Gets the previous-to-last <see cref="Point"/> in the <see cref="Sultanate"/> that had <see cref="Meeple">Meeples</see> picked up or dropped.
        /// </summary>
        public Point PreviousPoint
        {
            get { return this.previousPoint; }
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> discard pile.
        /// </summary>
        public EnumCollection<Resource> ResourceDiscards
        {
            get { return this.resourceDiscards; }
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> draw pile.
        /// </summary>
        public EnumCollection<Resource> ResourcePile
        {
            get { return this.resourcePile; }
        }

        /// <summary>
        /// Gets the <see cref="ScoreTable">ScoreTables</see> for all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, ScoreTable> ScoreTables
        {
            get { return this.scoreTables; }
        }

        /// <summary>
        /// Gets the <see cref="Sultanate"/>.
        /// </summary>
        public ImmutableList<Square> Sultanate
        {
            get { return this.sultanate; }
        }

        /// <summary>
        /// Gets the Turn Order Track.
        /// </summary>
        public ImmutableList<PlayerToken> TurnOrderTrack
        {
            get { return this.turnOrderTrack; }
        }

        /// <summary>
        /// Gets the face-up <see cref="Djinn">Djinns</see>.
        /// </summary>
        public ImmutableList<Djinn> VisibleDjinns
        {
            get { return this.visibleDjinns; }
        }

        /// <summary>
        /// Gets the face-up <see cref="Resource">Resources</see>.
        /// </summary>
        public ImmutableList<Resource> VisibleResources
        {
            get { return this.visibleResources; }
        }

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
            var nextIndex = this.turnOrderTrack.Count - 1;
            while (nextIndex >= 0 && this.turnOrderTrack[nextIndex] == null)
            {
                nextIndex--;
            }

            return nextIndex;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken playerToken)
        {
            var moves = new List<Move>();

            if (this.subsequentMovesFactory != null)
            {
                moves.AddRange(this.subsequentMoves.Value.Where(p => p.PlayerToken == playerToken));
            }
            else
            {
                var activePlayer = this.ActivePlayer;
                if (playerToken == activePlayer)
                {
                    switch (this.phase)
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
                }

                foreach (var djinn in this.inventory[playerToken].Djinns)
                {
                    moves.AddRange(djinn.GetMoves(this));
                }
            }

            var originalMoves = moves.ToImmutableList();
            foreach (var djinn in this.inventory[playerToken].Djinns)
            {
                moves.AddRange(djinn.GetAdditionalMoves(this, originalMoves));
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
            var inventory = this.inventory[playerToken];
            var scoreTable = this.scoreTables[playerToken];
            var owned = this.sultanate.Where(b => b.Owner == playerToken).ToList();
            var viziers = inventory.Meeples.Count(m => m == Meeple.Vizier);
            var playersWithFewerViziers = this.inventory.Values.Count(i => i.Meeples.Count(m => m == Meeple.Vizier) < viziers);
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
            if (this.phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            return this.players
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
            return this.sultanate.Count(s => s.Owner == playerToken) < this.CamelLimit;
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
                assassinationTables ?? this.assassinationTables,
                bag ?? this.bag,
                bidOrderTrack ?? this.bidOrderTrack,
                djinnDiscards ?? this.djinnDiscards,
                djinnPile ?? this.djinnPile,
                inHand ?? this.inHand,
                inventory ?? this.inventory,
                lastPoint ?? this.lastPoint,
                phase ?? this.phase,
                players ?? this.players,
                previousPoint ?? this.previousPoint,
                resourceDiscards ?? this.resourceDiscards,
                resourcePile ?? this.resourcePile,
                scoreTables ?? this.scoreTables,
                null,
                sultanate ?? this.sultanate,
                turnOrderTrack ?? this.turnOrderTrack,
                visibleDjinns ?? this.visibleDjinns,
                visibleResources ?? this.visibleResources);
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
                this.assassinationTables,
                this.bag,
                this.bidOrderTrack,
                this.djinnDiscards,
                this.djinnPile,
                this.inHand,
                this.inventory,
                this.lastPoint,
                this.phase,
                this.players,
                this.previousPoint,
                this.resourceDiscards,
                this.resourcePile,
                this.scoreTables,
                subsequentMoves,
                this.sultanate,
                this.turnOrderTrack,
                this.visibleDjinns,
                this.visibleResources);
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
                this.assassinationTables,
                this.bag,
                this.bidOrderTrack,
                this.djinnDiscards,
                this.djinnPile,
                this.inHand,
                this.inventory,
                this.lastPoint,
                this.phase,
                this.players,
                this.previousPoint,
                this.resourceDiscards,
                this.resourcePile,
                this.scoreTables,
                null,
                this.sultanate,
                this.turnOrderTrack,
                this.visibleDjinns,
                this.visibleResources);
        }

        private static IEnumerable<Move> GetAssassinationMoves(GameState state, int slaves = 0)
        {
            var player0 = state.ActivePlayer;

            Func<EnumCollection<Meeple>, IEnumerable<EnumCollection<Meeple>>> combos = killable =>
            {
                return killable.Combinations(Math.Min(killable.Count, state.assassinationTables[player0].KillCount));
            };

            foreach (var victim in state.players.Except(player0))
            {
                if (state.assassinationTables[victim].HasProtection)
                {
                    continue;
                }

                foreach (var kill in combos(state.inventory[victim].Meeples))
                {
                    yield return new AssassinatePlayerMove(state, victim, kill, s => s.With(phase: Phase.TileAction));
                }
            }

            foreach (var point in FiveTribes.Sultanate.GetPointsWithin(state.lastPoint, state.inHand.Count + slaves))
            {
                foreach (var kill in combos(state.sultanate[point].Meeples))
                {
                    yield return new AssassinateMove(state, point, kill, s => s.With(phase: Phase.TileAction));
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

                foreach (var player in newState.players)
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

            for (var i = 2; i < this.turnOrderTrack.Count; i++)
            {
                if (this.turnOrderTrack[i] == null && this.inventory[this.ActivePlayer].GoldCoins >= TurnOrderTrackCosts[i])
                {
                    var j = i == 2 && this.turnOrderTrack[0] == null ? 0 :
                            i == 2 && this.turnOrderTrack[1] == null ? 1 :
                            i;

                    yield return new BidMove(this, j, TurnOrderTrackCosts[j]);
                }
            }
        }

        private IEnumerable<Move> GetMerchandiseSaleMoves()
        {
            var keys = this.inventory[this.ActivePlayer].Resources.Keys.ToImmutableList().RemoveAll(r => r == Resource.Slave);

            for (var i = (1 << keys.Count) - 1; i > 0; i--)
            {
                var resources = new EnumCollection<Resource>(keys.Select((k, j) => new { k, j }).Where(x => (i & 1 << x.j) != 0).Select(x => x.k));

                yield return new SellMerchandiseMove(this, resources);
            }

            yield return new EndTurnMove(this);
        }

        private IEnumerable<Move> GetMoveMeeplesMoves()
        {
            var drops = this.sultanate.GetMoves(this.lastPoint, this.previousPoint, this.inHand);
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
            foreach (var point in this.sultanate.GetPickUps())
            {
                any = true;
                yield return new PickUpMeeplesMove(this, point);
            }

            if (!any)
            {
                yield return new ChangePhaseMove(this, "Skip move", Phase.MerchandiseSale);
            }
        }

        private IEnumerable<Move> GetTileActionMoves()
        {
            return this.sultanate[this.lastPoint].Tile.GetTileActionMoves(this);
        }

        private IEnumerable<Move> GetTileControlCheckMoves()
        {
            yield return new PlaceCamelMove(this, this.LastPoint, s => s.With(phase: Phase.TribesAction));
        }

        private IEnumerable<Move> GetTribesActionMoves()
        {
            var activePlayer = this.ActivePlayer;
            var tribe = this.inHand.Keys.Single();
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
                        yield return new ChangePhaseMove(this, "Skip Tribes Action", Phase.TileAction);
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
