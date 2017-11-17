// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Implements the game of Lotus.
    /// </summary>
    public class GameState : IGameState<Move>
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
        public GameState(int players)
        {
            if (players < MinPlayers || players > MaxPlayers)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            this.Players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableList();
            this.ActivePlayer = this.Players[0];
            this.Phase = Phase.Play;
            this.Inventory = Enumerable.Range(0, players).ToImmutableDictionary(i => this.Players[i], i => new Inventory(MakeDeck(this.Players[i], players)));
            this.RemainingActions = ActionsPerTurn;
            this.Field = ImmutableDictionary.CreateRange(Enum.GetValues(typeof(FlowerType)).Cast<FlowerType>().Select(f => new KeyValuePair<FlowerType, Flower>(f, new Flower())));
            this.ChoosingPlayers = ImmutableList<PlayerToken>.Empty;
            this.WildflowerDeck = InitialWildflowers.Deal(4, out ImmutableList<PetalCard> availableWildflowers);
            this.AvailableWildflowers = availableWildflowers;
        }

        private GameState(
            ImmutableList<PlayerToken> players,
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
        /// Gets the phase of the game.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players { get; }

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
            var points = flower.Petals.Where(p => p.Owner != null).Select(p => new { p.Owner, p.Guardians }).Concat(flower.Guardians.Select(g => new { Owner = g, Guardians = 1 }));
            var playerPoints = from p in points
                               group p.Guardians by p.Owner into g
                               select new { Owner = g.Key, Guardians = g.Sum() };
            return playerPoints
                .GroupBy(p => p.Guardians, p => p.Owner)
                .OrderByDescending(g => g.Key)
                .First()
                .ToImmutableList();
        }

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
                (comp = this.RemainingActions.CompareTo(state.RemainingActions)) != 0 ||
                (comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0)
            {
                return comp;
            }

            if (this.Players != state.Players)
            {
                if ((comp = this.Players.Count.CompareTo(state.Players.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.Players.Count; i++)
                {
                    if ((comp = this.Players[i].CompareTo(state.Players[i])) != 0)
                    {
                        return comp;
                    }
                }
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

            if (this.ChoosingPlayers != state.ChoosingPlayers)
            {
                if ((comp = this.ChoosingPlayers.Count.CompareTo(state.ChoosingPlayers.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.ChoosingPlayers.Count; i++)
                {
                    if ((comp = this.ChoosingPlayers[i].CompareTo(state.ChoosingPlayers[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.Field != state.Field)
            {
                foreach (FlowerType flowerType in Enum.GetValues(typeof(FlowerType)))
                {
                    if ((comp = this.Field[flowerType].CompareTo(state.Field[flowerType])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.AvailableWildflowers != state.AvailableWildflowers)
            {
                if ((comp = this.AvailableWildflowers.Count.CompareTo(state.AvailableWildflowers.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.AvailableWildflowers.Count; i++)
                {
                    var wildflower = this.AvailableWildflowers[i];
                    var otherWildflower = state.AvailableWildflowers[i];

                    if (wildflower == null && otherWildflower != null)
                    {
                        return -1;
                    }

                    if ((comp = wildflower.CompareTo(otherWildflower)) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.WildflowerDeck != state.WildflowerDeck)
            {
                if ((comp = this.WildflowerDeck.Count.CompareTo(state.WildflowerDeck.Count)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.WildflowerDeck.Count; i++)
                {
                    if ((comp = this.WildflowerDeck[i].CompareTo(state.WildflowerDeck[i])) != 0)
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
            return inventory.ScoringTokens * 5 + inventory.ScoringPile.Count;
        }

        /// <inheritdoc />
        public IGameState<Move> GetView(PlayerToken playerToken)
        {
            // TODO: Hide hands from other players.
            return this;
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
            ImmutableDictionary<PlayerToken, Inventory> inventory = null,
            int? remainingActions = null,
            ImmutableDictionary<FlowerType, Flower> field = null,
            ImmutableList<PlayerToken> choosingPlayers = null,
            ImmutableList<PetalCard> wildflowerDeck = null,
            ImmutableList<PetalCard> availableWildflowers = null)
        {
            return new GameState(
                players ?? this.Players,
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
            int remove = 0;
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
