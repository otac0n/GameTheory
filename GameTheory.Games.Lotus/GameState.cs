// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Lotus.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// The number of actions each player may perform on each turn.
        /// </summary>
        public const int ActionsPerTurn = 2;

        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 4;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        private static readonly ImmutableList<PetalCard> InitialWildflowers;

        static GameState()
        {
            InitialWildflowers =
                Enumerable.Range(0, 4).Select(_ => PetalCard.WildIris).Concat(
                Enumerable.Range(0, 4).Select(_ => PetalCard.WildPrimrose)).Concat(
                Enumerable.Range(0, 4).Select(_ => PetalCard.WildCherryBlossom)).Concat(
                Enumerable.Range(0, 4).Select(_ => PetalCard.WildLily)).Concat(
                Enumerable.Range(0, 4).Select(_ => PetalCard.WildLotus)).ToImmutableList();
        }

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
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Play;
            this.Inventory = Enumerable.Range(0, players).ToImmutableDictionary(i => this.Players[i], i => new Inventory(MakeDeck(this.Players[i], players)));
            this.RemainingActions = ActionsPerTurn;
            this.Field = ImmutableDictionary.CreateRange(EnumUtilities<FlowerType>.Values.Select(f => new KeyValuePair<FlowerType, Flower>(f, new Flower())));
            this.ChoosingPlayers = ImmutableList<PlayerToken>.Empty;
            this.WildflowerDeck = InitialWildflowers.Deal(4, out ImmutableList<PetalCard> availableWildflowers).Shuffle().ToImmutableList();
            this.AvailableWildflowers = availableWildflowers;
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableDictionary<PlayerToken, Inventory> inventory,
            int remainingActions,
            ImmutableDictionary<FlowerType, Flower> field,
            ImmutableList<PlayerToken> choosingPlayers,
            ImmutableList<PetalCard> wildflowerDeck,
            ImmutableList<PetalCard> availableWildflowers)
        {
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Inventory = inventory;
            this.RemainingActions = remainingActions;
            this.Field = field;
            this.ChoosingPlayers = choosingPlayers;
            this.WildflowerDeck = wildflowerDeck;
            this.AvailableWildflowers = availableWildflowers;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the currently available wildflowers.
        /// </summary>
        public ImmutableList<PetalCard> AvailableWildflowers { get; }

        /// <summary>
        /// Gets the players who are choosing a reward in the current sate.
        /// </summary>
        public ImmutableList<PlayerToken> ChoosingPlayers { get; }

        /// <summary>
        /// Gets the contents of the playing area.
        /// </summary>
        public ImmutableDictionary<FlowerType, Flower> Field { get; }

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

        /// <summary>
        /// Gets the number of remaining actions for the active player.
        /// </summary>
        public int RemainingActions { get; }

        /// <summary>
        /// Gets the deck of available wildflowers.
        /// </summary>
        public ImmutableList<PetalCard> WildflowerDeck { get; }

        /// <summary>
        /// Gets the players who control the specified <see cref="Flower"/>.
        /// </summary>
        /// <param name="flower">The <see cref="Flower"/> to evaluate.</param>
        /// <returns>The collection of players that control the specified flower.</returns>
        public static ImmutableList<PlayerToken> GetControllingPlayers(Flower flower)
        {
            if (flower == null)
            {
                throw new ArgumentNullException(nameof(flower));
            }

            var points = flower.Petals.Where(p => p.Owner != null).Select(p => new { p.Owner, p.Guardians }).Concat(flower.Guardians.Select(g => new { Owner = g, Guardians = 1 }));
            var playerPoints = from p in points
                               group p.Guardians by p.Owner into g
                               select new { Owner = g.Key, Guardians = g.Sum() };
            var controllingPlayers = playerPoints
                .GroupBy(p => p.Guardians, p => p.Owner)
                .OrderByDescending(g => g.Key)
                .Select(g => g.ToImmutableList())
                .FirstOrDefault();

            return controllingPlayers ?? ImmutableList<PlayerToken>.Empty;
        }

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
                (comp = this.RemainingActions.CompareTo(state.RemainingActions)) != 0 ||
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

            if ((comp = CompareUtilities.CompareLists(this.ChoosingPlayers, state.ChoosingPlayers)) != 0)
            {
                return comp;
            }

            if (this.Field != state.Field)
            {
                foreach (var flowerType in EnumUtilities<FlowerType>.Values)
                {
                    if ((comp = this.Field[flowerType].CompareTo(state.Field[flowerType])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if ((comp = CompareUtilities.CompareLists(this.AvailableWildflowers, state.AvailableWildflowers)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.WildflowerDeck, state.WildflowerDeck)) != 0)
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
                    moves.AddRange(Moves.PlayPetalCardsMove.GenerateMoves(this));
                    moves.AddRange(Moves.ExchangePetalCardsMove.GenerateMoves(this));
                    moves.AddRange(Moves.MoveGuardianMove.GenerateMoves(this));
                    break;

                case Phase.Draw:
                    moves.AddRange(Moves.DrawCardMove.GenerateMoves(this));
                    moves.AddRange(Moves.TakeWildflowerMove.GenerateMoves(this));
                    break;

                case Phase.ClaimReward:
                    moves.AddRange(Moves.ClaimSpecialPowerMove.GenerateMoves(this));
                    moves.AddRange(Moves.ClaimScoringTokenMove.GenerateMoves(this));
                    break;

                case Phase.End:
                    break;
            }

            return moves.ToImmutable();
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="player">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken player)
        {
            var inventory = this.Inventory[player];
            return inventory.ScoringTokens * 5 + inventory.ScoringPile.Count;
        }

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            var shuffler = new GameShuffler<GameState>(this);

            shuffler.AddCollection(
                "Wildflower",
                this.WildflowerDeck,
                (state, value) => state.With(
                    wildflowerDeck: value.ToImmutableList()));

            foreach (var p in this.Players)
            {
                var player = p;
                var groupPrefix = $"{player.Id}/";

                for (var i = 0; i < this.Inventory[player].Deck.Count; i++)
                {
                    var index = i;
                    var cardGroup = (this.Inventory[player].Deck[index].Owner?.Id)?.ToString() ?? "Wildflower";
                    shuffler.Add(
                        $"{groupPrefix}{cardGroup}",
                        this.Inventory[player].Deck[index],
                        (state, value) => state.With(
                            inventory: state.Inventory.SetItem(
                                player,
                                state.Inventory[player].With(
                                    deck: state.Inventory[player].Deck.SetItem(index, value)))));
                }

                if (player == playerToken)
                {
                    continue;
                }

                for (var i = 0; i < this.Inventory[player].Hand.Count; i++)
                {
                    var index = i;
                    var cardGroup = (this.Inventory[player].Hand[index].Owner?.Id)?.ToString() ?? "Wildflower";
                    shuffler.Add(
                        $"{groupPrefix}{cardGroup}",
                        this.Inventory[player].Hand[index],
                        (state, value) => state.With(
                            inventory: state.Inventory.SetItem(
                                player,
                                state.Inventory[player].With(
                                    hand: state.Inventory[player].Hand.SetItem(index, value)))));
                }
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

            return this.Players
                .GroupBy(p =>
                {
                    var inventory = this.Inventory[p];
                    return new { score = this.GetScore(p), cards = inventory.Hand.Count + inventory.Deck.Count };
                })
                .OrderByDescending(g => g.Key.score)
                .ThenByDescending(g => g.Key.cards)
                .First()
                .ToImmutableList();
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
            ImmutableDictionary<PlayerToken, Inventory> inventory = null,
            int? remainingActions = null,
            ImmutableDictionary<FlowerType, Flower> field = null,
            ImmutableList<PlayerToken> choosingPlayers = null,
            ImmutableList<PetalCard> wildflowerDeck = null,
            ImmutableList<PetalCard> availableWildflowers = null)
        {
            return new GameState(
                this.Players,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                inventory ?? this.Inventory,
                remainingActions ?? this.RemainingActions,
                field ?? this.Field,
                choosingPlayers ?? this.ChoosingPlayers,
                wildflowerDeck ?? this.WildflowerDeck,
                availableWildflowers ?? this.AvailableWildflowers);
        }

        private static ImmutableList<PetalCard> MakeDeck(PlayerToken player, int players)
        {
            var remove = 0;
            switch (players)
            {
                case 4:
                    remove = 2;
                    break;

                case 3:
                    remove = 1;
                    break;
            }

            var singleGuardianIris = new PetalCard(FlowerType.Iris, player, 1);
            var singleGuardianPrimrose = new PetalCard(FlowerType.Primrose, player, 1);
            var singleGuardianCherryBlossom = new PetalCard(FlowerType.CherryBlossom, player, 1);
            var singleGuardianLily = new PetalCard(FlowerType.Lily, player, 1);
            var singleGuardianLotus = new PetalCard(FlowerType.Lotus, player, 1);

            return
                new[]
                {
                    new PetalCard(FlowerType.Iris, player, 2),
                    new PetalCard(FlowerType.Primrose, player, 2),
                    new PetalCard(FlowerType.CherryBlossom, player, 2),
                    new PetalCard(FlowerType.Lily, player, 2),
                    new PetalCard(FlowerType.Lotus, player, 2),
                }.Concat(
                Enumerable.Range(0, 4 - remove).Select(_ => singleGuardianIris)).Concat(
                Enumerable.Range(0, 5 - remove).Select(_ => singleGuardianPrimrose)).Concat(
                Enumerable.Range(0, 6 - remove).Select(_ => singleGuardianCherryBlossom)).Concat(
                Enumerable.Range(0, 6 - remove).Select(_ => singleGuardianLily)).Concat(
                Enumerable.Range(0, 5 - remove).Select(_ => singleGuardianLotus)).ToImmutableList();
        }
    }
}
