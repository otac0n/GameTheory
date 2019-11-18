// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using GameTheory.Games.SevenDragons.Cards;
    using GameTheory.Games.SevenDragons.Moves;

    /// <summary>
    /// Represents the current state in a game of Seven Dragons.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 5;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        /// <summary>
        /// The size of the starting hand.
        /// </summary>
        public const int StartingHandSize = 3;

        /// <summary>
        /// The minimum group size to win the game.
        /// </summary>
        public const int WinningGroupSize = 7;

        private static readonly ImmutableList<Card> ActionCards = ImmutableList.Create<Card>(
            ZapACardCard.Instance,
            TradeGoalsCard.Instance,
            RotateGoalsCard.Instance,
            MoveACardCard.Instance,
            TradeHandsCard.Instance);

        private static readonly ImmutableList<Card> DragonCards = ImmutableList.Create<Card>(
            new DragonCard(Color.Rainbow),
            new DragonCard(Color.Red, Color.Red, Color.Red, Color.Red),
            new DragonCard(Color.Red, Color.Red, Color.Red, Color.Red),
            new DragonCard(Color.Red, Color.Red, Color.Gold, Color.Gold),
            new DragonCard(Color.Red, Color.Red, Color.Blue, Color.Blue),
            new DragonCard(Color.Red, Color.Red, Color.Black, Color.Green),
            new DragonCard(Color.Red, Color.Gold, Color.Red, Color.Gold),
            new DragonCard(Color.Red, Color.Gold, Color.Red, Color.Blue),
            new DragonCard(Color.Red, Color.Black, Color.Red, Color.Black),
            new DragonCard(Color.Red, Color.Black, Color.Blue, Color.Green),
            new DragonCard(Color.Gold, Color.Gold, Color.Red, Color.Black),
            new DragonCard(Color.Gold, Color.Gold, Color.Gold, Color.Gold),
            new DragonCard(Color.Gold, Color.Gold, Color.Gold, Color.Gold),
            new DragonCard(Color.Gold, Color.Gold, Color.Green, Color.Green),
            new DragonCard(Color.Gold, Color.Gold, Color.Black, Color.Black),
            new DragonCard(Color.Gold, Color.Blue, Color.Red, Color.Black),
            new DragonCard(Color.Gold, Color.Blue, Color.Gold, Color.Blue),
            new DragonCard(Color.Gold, Color.Blue, Color.Gold, Color.Green),
            new DragonCard(Color.Gold, Color.Green, Color.Gold, Color.Green),
            new DragonCard(Color.Gold, Color.Green, Color.Blue, Color.Red),
            new DragonCard(Color.Gold, Color.Black, Color.Green, Color.Blue),
            new DragonCard(Color.Blue, Color.Red, Color.Blue, Color.Red),
            new DragonCard(Color.Blue, Color.Red, Color.Green, Color.Black),
            new DragonCard(Color.Blue, Color.Blue, Color.Gold, Color.Red),
            new DragonCard(Color.Blue, Color.Blue, Color.Gold, Color.Gold),
            new DragonCard(Color.Blue, Color.Blue, Color.Blue, Color.Blue),
            new DragonCard(Color.Blue, Color.Blue, Color.Blue, Color.Blue),
            new DragonCard(Color.Blue, Color.Blue, Color.Black, Color.Black),
            new DragonCard(Color.Blue, Color.Green, Color.Blue, Color.Black),
            new DragonCard(Color.Blue, Color.Black, Color.Gold, Color.Red),
            new DragonCard(Color.Blue, Color.Black, Color.Blue, Color.Black),
            new DragonCard(Color.Green, Color.Red, Color.Gold, Color.Blue),
            new DragonCard(Color.Green, Color.Red, Color.Green, Color.Red),
            new DragonCard(Color.Green, Color.Red, Color.Black, Color.Gold),
            new DragonCard(Color.Green, Color.Gold, Color.Blue, Color.Black),
            new DragonCard(Color.Green, Color.Blue, Color.Green, Color.Blue),
            new DragonCard(Color.Green, Color.Green, Color.Red, Color.Red),
            new DragonCard(Color.Green, Color.Green, Color.Blue, Color.Gold),
            new DragonCard(Color.Green, Color.Green, Color.Blue, Color.Blue),
            new DragonCard(Color.Green, Color.Green, Color.Green, Color.Green),
            new DragonCard(Color.Green, Color.Green, Color.Green, Color.Green),
            new DragonCard(Color.Green, Color.Black, Color.Green, Color.Red),
            new DragonCard(Color.Black, Color.Red, Color.Black, Color.Gold),
            new DragonCard(Color.Black, Color.Gold, Color.Black, Color.Gold),
            new DragonCard(Color.Black, Color.Green, Color.Gold, Color.Red),
            new DragonCard(Color.Black, Color.Green, Color.Black, Color.Green),
            new DragonCard(Color.Black, Color.Black, Color.Red, Color.Red),
            new DragonCard(Color.Black, Color.Black, Color.Green, Color.Blue),
            new DragonCard(Color.Black, Color.Black, Color.Green, Color.Green),
            new DragonCard(Color.Black, Color.Black, Color.Black, Color.Black),
            new DragonCard(Color.Black, Color.Black, Color.Black, Color.Black));

        private static readonly ImmutableList<Color> Goals = ImmutableList.Create(Color.Red, Color.Gold, Color.Blue, Color.Green, Color.Black);

        private static readonly ImmutableDictionary<Point, DragonCard> StartingTable =
            ImmutableDictionary<Point, DragonCard>.Empty
                .Add(new Point(0, 0), new DragonCard(Color.Silver));

        private readonly InterstitialState interstitialState;
        private readonly Lazy<ImmutableList<Move>> subsequentMoves;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="players">The number of players.</param>
        /// <param name="includeActions">A value that indicates whether or not acion cards are included.</param>
        public GameState([Range(MinPlayers, MaxPlayers)] int players = MinPlayers, bool includeActions = true)
            : this(null)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableArray();
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Draw;
            this.Table = StartingTable;
            this.DiscardPile = ImmutableList<ActionCard>.Empty;

            var inventoryOffset = GameTheory.Random.Instance.Next(players);
            this.InventoryMap = this.Players.Select((p, i) => new { Player = p, Index = (i + inventoryOffset) % players }).ToImmutableDictionary(x => x.Player, x => x.Index);
            var inventories = Goals.Shuffle().Select(goal => new Inventory(goal, ImmutableList<Card>.Empty)).ToImmutableList();

            var deck = includeActions ? DragonCards.AddRange(ActionCards) : DragonCards;
            foreach (var ix in this.InventoryMap.Values)
            {
                deck = deck.Deal(StartingHandSize, out var dealt);
                inventories = inventories.SetItem(ix, inventories[ix].With(hand: dealt));
            }

            this.Deck = deck.Shuffle().ToImmutableList();
            this.Inventories = inventories;
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            ImmutableDictionary<PlayerToken, int> inventoryMap,
            InterstitialState interstitialState,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableList<Card> deck,
            ImmutableList<ActionCard> discardPile,
            ImmutableList<Inventory> inventories,
            ImmutableDictionary<Point, DragonCard> table)
            : this(interstitialState)
        {
            this.Players = players;
            this.InventoryMap = inventoryMap;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Deck = deck;
            this.DiscardPile = discardPile;
            this.Inventories = inventories;
            this.Table = table;
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
        /// Gets the <see cref="Card"/> deck.
        /// </summary>
        public ImmutableList<Card> Deck { get; }

        /// <summary>
        /// Gets the <see cref="Card"/> discard pile.
        /// </summary>
        public ImmutableList<ActionCard> DiscardPile { get; }

        /// <summary>
        /// Gets the <see cref="Inventory">Inventories</see> for all players.
        /// </summary>
        public ImmutableList<Inventory> Inventories { get; }

        /// <summary>
        /// Gets a lookup from <see cref="PlayerToken"/> to index in <see cref="Inventories"/>.
        /// </summary>
        public ImmutableDictionary<PlayerToken, int> InventoryMap { get; }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the mapping of players to their inventories.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <summary>
        /// Gets the table to which the cards are played.
        /// </summary>
        public ImmutableDictionary<Point, DragonCard> Table { get; }

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

            if ((comp = EnumComparer<Phase>.Default.Compare(this.Phase, state.Phase)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0 ||
                (comp = CompareUtilities.CompareDictionaries(this.InventoryMap, state.InventoryMap)) != 0 ||
                (comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Inventories, state.Inventories)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Deck, state.Deck)) != 0 ||
                (comp = CompareUtilities.CompareDictionaries(this.Table, state.Table)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.DiscardPile, state.DiscardPile)) != 0)
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
                        moves.AddRange(DrawCardsMove.GenerateMoves(this));
                        break;

                    case Phase.Play:
                        moves.AddRange(PlaceCardMove.GenerateMoves(this));
                        moves.AddRange(ActionCard.GenerateMoves(this));
                        break;
                }
            }

            return moves.ToImmutableList();
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

            // TODO: Shuffle.
            return shuffler.Take(maxStates);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            var colorSet = new HashSet<Color>(
                (from c in this.GetConnections()
                 from g in c.Value
                 let count = g.Select(y => y.Item1).Distinct().Count()
                 group c.Key by count into g
                 orderby g.Key descending
                 select g).First());

            var winners = (from p in this.Players
                           where colorSet.Contains(this.Inventories[this.InventoryMap[p]].Goal)
                           group p by p == this.ActivePlayer into g
                           orderby g.Key descending
                           select g).FirstOrDefault();

            return (winners == null ? Enumerable.Empty<PlayerToken>() : winners).ToImmutableList();
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

            if (this.CompareTo(move.GameState) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(move));
            }

            return move.Apply(this);
        }

        internal IEnumerable<KeyValuePair<Color, List<List<Tuple<Point, Point>>>>> GetConnections()
        {
            var queue = new Queue<Tuple<Point, Point>>();
            var groups = new Dictionary<Tuple<Point, Point>, int>();
            var rainbowKeys = new List<Tuple<Point, Point>>();
            foreach (var point1 in this.Table.Keys)
            {
                foreach (var point2 in DragonCard.Grid)
                {
                    var key = Tuple.Create(point1, point2);
                    queue.Enqueue(key);
                    groups[key] = groups.Count;
                    if (this.GetColor(key, Color.Rainbow) == Color.Rainbow)
                    {
                        rainbowKeys.Add(key);
                    }
                }
            }

            this.ReduceGroups(groups, queue, Color.Rainbow);

            foreach (var goal in Goals)
            {
                Dictionary<Tuple<Point, Point>, int> goalGroups;
                if (rainbowKeys.Count > 0)
                {
                    goalGroups = new Dictionary<Tuple<Point, Point>, int>(groups);
                    var goalQueue = new Queue<Tuple<Point, Point>>(rainbowKeys);

                    this.ReduceGroups(goalGroups, goalQueue, goal);
                }
                else
                {
                    goalGroups = groups;
                }

                yield return new KeyValuePair<Color, List<List<Tuple<Point, Point>>>>(
                    goal,
                    (from g in goalGroups
                     group g.Key by g.Value into g
                     where this.GetColor(g.First(), goal) == goal
                     select g.ToList()).ToList());
            }
        }

        internal HashSet<Point> GetEmptyAdjacent()
        {
            var adjacent = new HashSet<Point>();
            adjacent.UnionWith(this.Table.Keys.SelectMany(k => new[] { new Point(k.X + 1, k.Y), new Point(k.X - 1, k.Y), new Point(k.X, k.Y + 1), new Point(k.X, k.Y - 1) }));
            adjacent.ExceptWith(this.Table.Keys);
            return adjacent;
        }

        internal IList<Tuple<Point, Point>> GetMatchingAdjacent(Point point1, Point point2, Color matchColor, Color rainbowColor)
        {
            var adjacent = new List<Tuple<Point, Point>>
            {
                point2.X > 0 ? Tuple.Create(point1, new Point(point2.X - 1, point2.Y)) : Tuple.Create(new Point(point1.X - 1, point1.Y), new Point(DragonCard.Grid.Width - 1, point2.Y)),
                point2.X < DragonCard.Grid.Width - 1 ? Tuple.Create(point1, new Point(point2.X + 1, point2.Y)) : Tuple.Create(new Point(point1.X + 1, point1.Y), new Point(0, point2.Y)),
                point2.Y > 0 ? Tuple.Create(point1, new Point(point2.X, point2.Y - 1)) : Tuple.Create(new Point(point1.X, point1.Y - 1), new Point(point2.X, DragonCard.Grid.Height - 1)),
                point2.Y < DragonCard.Grid.Height - 1 ? Tuple.Create(point1, new Point(point2.X, point2.Y + 1)) : Tuple.Create(new Point(point1.X, point1.Y + 1), new Point(point2.X, 0)),
            };
            adjacent.RemoveAll(adj => this.GetColor(adj, rainbowColor) != matchColor);
            return adjacent;
        }

        internal GameState With(
            PlayerToken activePlayer = null,
            Phase? phase = null,
            ImmutableList<Card> deck = null,
            ImmutableList<ActionCard> discardPile = null,
            ImmutableList<Inventory> inventories = null,
            ImmutableDictionary<Point, DragonCard> table = null)
        {
            return new GameState(
                this.Players,
                this.InventoryMap,
                null,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                deck ?? this.Deck,
                discardPile ?? this.DiscardPile,
                inventories ?? this.Inventories,
                table ?? this.Table);
        }

        internal GameState WithInterstitialState(InterstitialState interstitialState)
        {
            return new GameState(
                this.Players,
                this.InventoryMap,
                interstitialState,
                this.ActivePlayer,
                this.Phase,
                this.Deck,
                this.DiscardPile,
                this.Inventories,
                this.Table);
        }

        private Color? GetColor(Tuple<Point, Point> key, Color rainbowColor)
        {
            if (!this.Table.TryGetValue(key.Item1, out var card))
            {
                return null;
            }

            var color = card.Colors[DragonCard.Grid.IndexOf(key.Item2)];
            if (color == Color.Silver)
            {
                if (this.DiscardPile.Count > 0)
                {
                    color = this.DiscardPile[this.DiscardPile.Count - 1].Color;
                }
                else
                {
                    color = Color.Rainbow;
                }
            }

            return color == Color.Rainbow ? rainbowColor : color;
        }

        private void ReduceGroups(Dictionary<Tuple<Point, Point>, int> groups, Queue<Tuple<Point, Point>> queue, Color rainbowColor)
        {
            while (queue.Count > 0)
            {
                var key = queue.Dequeue();
                var group = groups[key];
                var adjacent = this.GetMatchingAdjacent(key.Item1, key.Item2, this.GetColor(key, rainbowColor).Value, rainbowColor);

                var min = group;
                foreach (var adj in adjacent)
                {
                    min = Math.Min(groups[adj], min);
                }

                if (group > min)
                {
                    groups[key] = min;
                }

                foreach (var adj in adjacent)
                {
                    if (groups[adj] != min)
                    {
                        queue.Enqueue(adj);
                    }
                }
            }
        }
    }
}
