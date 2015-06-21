// -----------------------------------------------------------------------
// <copyright file="GameState.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using GameTheory.Games.FiveTribes.Djinns;
    using GameTheory.Games.FiveTribes.Moves;

    public class GameState : IGameState<Move>
    {
        public const int MaxPlayers = 4;
        public const int MinPlayers = 2;
        public const int TribesCount = 5;
        public static readonly ImmutableList<Tile> InitialTiles;
        public static readonly ImmutableList<int> SuitValues;
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
        private readonly Direction lastDirection;
        private readonly Point lastPoint;
        private readonly Phase phase;
        private readonly ImmutableList<PlayerToken> players;
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

            var smallSacredPlace = new Tile.SacredPlace(6);
            InitialTiles = ImmutableList.Create<Tile>(
                Tile.BigMarket.Instance,
                Tile.BigMarket.Instance,
                Tile.BigMarket.Instance,
                Tile.BigMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.SmallMarket.Instance,
                Tile.Oasis.Instance,
                Tile.Oasis.Instance,
                Tile.Oasis.Instance,
                Tile.Oasis.Instance,
                Tile.Oasis.Instance,
                Tile.Oasis.Instance,
                Tile.Village.Instance,
                Tile.Village.Instance,
                Tile.Village.Instance,
                Tile.Village.Instance,
                Tile.Village.Instance,
                smallSacredPlace,
                smallSacredPlace,
                smallSacredPlace,
                smallSacredPlace,
                new Tile.SacredPlace(10),
                new Tile.SacredPlace(12),
                new Tile.SacredPlace(15));

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
                ////Dhenim.Instance, // TODO: How do we handle this for tests?
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

        public GameState(int players)
            : this(null)
        {
            Contract.Requires(players >= MinPlayers && players <= MaxPlayers);
            this.players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableList();
            this.phase = Phase.Bid;
            this.bidOrderTrack = ImmutableQueue.CreateRange((players == 2 ? this.players.Concat(this.players) : this.players).Shuffle());
            this.turnOrderTrack = ImmutableList.CreateRange(new PlayerToken[TurnOrderTrackCosts.Count]);
            this.inventory = this.players.ToImmutableDictionary(p => p, p => new Inventory());
            this.assassinationTables = this.players.ToImmutableDictionary(p => p, p => new AssassinationTable());
            this.scoreTables = this.players.ToImmutableDictionary(p => p, p => new ScoreTable());
            this.sultanate = ImmutableList.CreateRange(InitialTiles.Shuffle().Zip(Meeples.Shuffle().Partition(3), (t, ms) => new Square(t, new EnumCollection<Meeple>(ms))));
            this.bag = EnumCollection<Meeple>.Empty;
            this.djinnPile = GameState.Djinns.Deal(3, out this.visibleDjinns);
            this.djinnDiscards = ImmutableList<Djinn>.Empty;
            this.resourcePile = GameState.Resources.Deal(9, out this.visibleResources);
            this.resourceDiscards = EnumCollection<Resource>.Empty;
            this.additionalState = ImmutableDictionary<string, string>.Empty;
        }

        internal GameState(ImmutableDictionary<string, string> additionalState, ImmutableDictionary<PlayerToken, AssassinationTable> assassinationTables, EnumCollection<Meeple> bag, ImmutableQueue<PlayerToken> bidOrderTrack, ImmutableList<PlayerToken> turnOrderTrack, ImmutableList<Square> sultanate, ImmutableList<Djinn> djinnDiscards, ImmutableList<Djinn> djinnPile, EnumCollection<Meeple> inHand, ImmutableDictionary<PlayerToken, Inventory> inventory, Direction lastDirection, Point lastPoint, Phase phase, ImmutableList<PlayerToken> players, EnumCollection<Resource> resourceDiscards, EnumCollection<Resource> resourcePile, ImmutableDictionary<PlayerToken, ScoreTable> scoreTables, Func<GameState, IEnumerable<Move>> subsequentMovesFactory, ImmutableList<Djinn> visibleDjinns, ImmutableList<Resource> visibleResources)
            : this(subsequentMovesFactory)
        {
            this.additionalState = additionalState;
            this.assassinationTables = assassinationTables;
            this.bag = bag;
            this.bidOrderTrack = bidOrderTrack;
            this.turnOrderTrack = turnOrderTrack;
            this.sultanate = sultanate;
            this.djinnDiscards = djinnDiscards;
            this.djinnPile = djinnPile;
            this.inHand = inHand;
            this.inventory = inventory;
            this.lastDirection = lastDirection;
            this.lastPoint = lastPoint;
            this.phase = phase;
            this.players = players;
            this.resourceDiscards = resourceDiscards;
            this.resourcePile = resourcePile;
            this.scoreTables = scoreTables;
            this.visibleDjinns = visibleDjinns;
            this.visibleResources = visibleResources;
        }

        private GameState(Func<GameState, IEnumerable<Move>> subsequentMovesFactory)
        {
            this.subsequentMovesFactory = subsequentMovesFactory;
            this.subsequentMoves = new Lazy<ImmutableList<Move>>(() => this.subsequentMovesFactory(this).ToImmutableList(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public PlayerToken ActivePlayer
        {
            get
            {
                return this.phase == Phase.End ? null :
                       this.phase == Phase.Bid ? this.bidOrderTrack.Peek() :
                       this.phase == Phase.MoveTurnMarker ? this.turnOrderTrack[this.GetHighestBidIndex()] :
                       this.bidOrderTrack.Last();
            }
        }

        public ImmutableDictionary<PlayerToken, AssassinationTable> AssassinationTables
        {
            get { return this.assassinationTables; }
        }

        public EnumCollection<Meeple> Bag
        {
            get { return this.bag; }
        }

        public ImmutableQueue<PlayerToken> BidOrderTrack
        {
            get { return this.bidOrderTrack; }
        }

        public int CamelLimit
        {
            get
            {
                return this.players.Count > 2 ? 8 : 11;
            }
        }

        public ImmutableList<Djinn> DjinnDiscards
        {
            get { return this.djinnDiscards; }
        }

        public ImmutableList<Djinn> DjinnPile
        {
            get { return this.djinnPile; }
        }

        public bool HasSubsequentMoves
        {
            get { return this.subsequentMovesFactory != null && !this.subsequentMoves.Value.IsEmpty; }
        }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players
        {
            get { return this.Players; }
        }

        public EnumCollection<Meeple> InHand
        {
            get { return this.inHand; }
        }

        public ImmutableDictionary<PlayerToken, Inventory> Inventory
        {
            get { return this.inventory; }
        }

        public Direction LastDirection
        {
            get { return this.lastDirection; }
        }

        public Point LastPoint
        {
            get { return this.lastPoint; }
        }

        public Phase Phase
        {
            get { return this.phase; }
        }

        public ImmutableList<PlayerToken> Players
        {
            get { return this.players; }
        }

        public EnumCollection<Resource> ResourceDiscards
        {
            get { return this.resourceDiscards; }
        }

        public EnumCollection<Resource> ResourcePile
        {
            get { return this.resourcePile; }
        }

        public ImmutableDictionary<PlayerToken, ScoreTable> ScoreTables
        {
            get { return this.scoreTables; }
        }

        public ImmutableList<Square> Sultanate
        {
            get { return this.sultanate; }
        }

        public ImmutableList<PlayerToken> TurnOrderTrack
        {
            get { return this.turnOrderTrack; }
        }

        public ImmutableList<Djinn> VisibleDjinns
        {
            get { return this.visibleDjinns; }
        }

        public ImmutableList<Resource> VisibleResources
        {
            get { return this.visibleResources; }
        }

        public string this[string stateKey]
        {
            get
            {
                string result;
                this.additionalState.TryGetValue(stateKey, out result);
                return result;
            }
        }

        public static int ScoreResources(EnumCollection<Resource> resources)
        {
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

        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken player)
        {
            var moves = new List<Move>();

            if (this.subsequentMovesFactory != null)
            {
                moves.AddRange(this.subsequentMoves.Value.Where(p => p.Player == player));
            }
            else
            {
                var activePlayer = this.ActivePlayer;
                if (player == activePlayer)
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

                        case Phase.CleanUp:
                            moves.AddRange(this.GetCleanUpMoves());
                            break;
                    }

                    moves.AddRange(this.GetMerchandiseSaleMoves());
                }

                foreach (var djinn in this.inventory[player].Djinns)
                {
                    moves.AddRange(djinn.GetMoves(this));
                }
            }

            var originalMoves = moves.ToImmutableList();
            foreach (var djinn in this.inventory[player].Djinns)
            {
                moves.AddRange(djinn.GetAdditionalMoves(this, originalMoves));
            }

            return moves.ToImmutableList();
        }

        public int GetHighestBidIndex()
        {
            var nextIndex = this.turnOrderTrack.Count - 1;
            while (nextIndex >= 0 && this.turnOrderTrack[nextIndex] == null)
            {
                nextIndex--;
            }

            return nextIndex;
        }

        public int GetScore(PlayerToken player)
        {
            var inventory = this.inventory[player];
            var scoreTable = this.scoreTables[player];
            var owned = this.sultanate.Where(b => b.Owner == player).ToList();
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

        IGameState<Move> IGameState<Move>.MakeMove(Move move)
        {
            return this.MakeMove(move);
        }

        public bool IsPlayerUnderCamelLimit(PlayerToken player)
        {
            return this.sultanate.Count(s => s.Owner == player) < this.CamelLimit;
        }

        public GameState MakeMove(Move move)
        {
            Contract.Requires(move.State == this);

            var newState = move.Apply(this);
            return HandleTransition(this, newState);
        }

        public GameState With(ImmutableDictionary<PlayerToken, AssassinationTable> assassinationTables = null, EnumCollection<Meeple> bag = null, ImmutableQueue<PlayerToken> bidOrderTrack = null, ImmutableList<PlayerToken> turnOrderTrack = null, ImmutableList<Square> sultanate = null, ImmutableList<Djinn> djinnDiscards = null, ImmutableList<Djinn> djinnPile = null, EnumCollection<Meeple> inHand = null, ImmutableDictionary<PlayerToken, Inventory> inventory = null, Direction? lastDirection = null, Point? lastPoint = null, Phase? phase = null, ImmutableList<PlayerToken> players = null, EnumCollection<Resource> resourceDiscards = null, EnumCollection<Resource> resourcePile = null, ImmutableDictionary<PlayerToken, ScoreTable> scoreTables = null, ImmutableList<Djinn> visibleDjinns = null, ImmutableList<Resource> visibleResources = null)
        {
            return new GameState(
                this.additionalState,
                assassinationTables ?? this.assassinationTables,
                bag ?? this.bag,
                bidOrderTrack ?? this.bidOrderTrack,
                turnOrderTrack ?? this.turnOrderTrack,
                sultanate ?? this.sultanate,
                djinnDiscards ?? this.djinnDiscards,
                djinnPile ?? this.djinnPile,
                inHand ?? this.inHand,
                inventory ?? this.inventory,
                lastDirection ?? this.lastDirection,
                lastPoint ?? this.lastPoint,
                phase ?? this.phase,
                players ?? this.players,
                resourceDiscards ?? this.resourceDiscards,
                resourcePile ?? this.resourcePile,
                scoreTables ?? this.scoreTables,
                null,
                visibleDjinns ?? this.visibleDjinns,
                visibleResources ?? this.visibleResources);
        }

        public GameState WithMoves(Func<GameState, Move> subsequentMoves)
        {
            return this.WithMoves(s => new[] { subsequentMoves(s) });
        }

        public GameState WithMoves(Func<GameState, IEnumerable<Move>> subsequentMoves)
        {
            return new GameState(
                this.additionalState,
                this.assassinationTables,
                this.bag,
                this.bidOrderTrack,
                this.turnOrderTrack,
                this.sultanate,
                this.djinnDiscards,
                this.djinnPile,
                this.inHand,
                this.inventory,
                this.lastDirection,
                this.lastPoint,
                this.phase,
                this.players,
                this.resourceDiscards,
                this.resourcePile,
                this.scoreTables,
                subsequentMoves,
                this.visibleDjinns,
                this.visibleResources);
        }

        public GameState WithState(string stateKey, string value)
        {
            return new GameState(
                value == null ? this.additionalState.Remove(stateKey) : this.additionalState.SetItem(stateKey, value),
                this.assassinationTables,
                this.bag,
                this.bidOrderTrack,
                this.turnOrderTrack,
                this.sultanate,
                this.djinnDiscards,
                this.djinnPile,
                this.inHand,
                this.inventory,
                this.lastDirection,
                this.lastPoint,
                this.phase,
                this.players,
                this.resourceDiscards,
                this.resourcePile,
                this.scoreTables,
                null,
                this.visibleDjinns,
                this.visibleResources);
        }

        private static IEnumerable<Move> GetAssassinationMoves(GameState state0, int slaves = 0)
        {
            var player0 = state0.ActivePlayer;

            Func<EnumCollection<Meeple>, IEnumerable<EnumCollection<Meeple>>> combos = killable =>
            {
                return killable.Combinations(Math.Min(killable.Count, state0.assassinationTables[player0].KillCount));
            };

            foreach (var victim in state0.players.Except(player0))
            {
                if (state0.assassinationTables[victim].HasProtection)
                {
                    continue;
                }

                foreach (var kill in combos(state0.inventory[victim].Meeples))
                {
                    yield return new AssassinatePlayerMove(state0, victim, kill, s => s.With(phase: Phase.TileAction));
                }
            }

            foreach (var point in FiveTribes.Sultanate.GetPointsWithin(state0.lastPoint, state0.inHand.Count + slaves))
            {
                foreach (var kill in combos(state0.sultanate[point].Meeples))
                {
                    yield return new AssassinateMove(state0, point, kill, s => s.With(phase: Phase.TileAction));
                }
            }
        }

        private static GameState HandleTransition(GameState oldState, GameState newState)
        {
            Contract.Requires(newState.subsequentMovesFactory == null || !newState.subsequentMoves.IsValueCreated);

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
            Contract.Requires(this.Phase == FiveTribes.Phase.Bid);

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

        private IEnumerable<Move> GetCleanUpMoves()
        {
            yield return new EndTurnMove(this);
        }

        private IEnumerable<Move> GetMerchandiseSaleMoves()
        {
            var keys = this.inventory[this.ActivePlayer].Resources.Keys.ToImmutableList().RemoveAll(r => r == Resource.Slave);

            for (var i = (1 << keys.Count) - 1; i > 0; i--)
            {
                var resources = new EnumCollection<Resource>(keys.Select((k, j) => new { k, j }).Where(x => (i & 1 << x.j) != 0).Select(x => x.k));

                yield return new SellMerchandiceMove(this, resources);
            }
        }

        private IEnumerable<Move> GetMoveMeeplesMoves()
        {
            var drops = this.sultanate.GetMoves(this.lastPoint, this.lastDirection, this.inHand);
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
            foreach (var point in this.sultanate.GetPickups())
            {
                any = true;
                yield return new PickUpMeeplesMove(this, point);
            }

            if (!any)
            {
                yield return new ChangePhaseMove(this, "Skip move", Phase.CleanUp);
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
