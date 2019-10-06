// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

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
        /// Gets the number of points needed to win the game.
        /// </summary>
        private static readonly ImmutableDictionary<int, int> PointThreshold;

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
            this.Phase = this.Phase;
            this.Deck = MakeDeck(players);
            this.Inventory = Enumerable.Range(0, players).ToImmutableDictionary(i => this.Players[i], i => new Inventory());
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            ImmutableList<Card> deck,
            ImmutableDictionary<PlayerToken, Inventory> inventory)
        {
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Deck = deck;
            this.Inventory = inventory;
        }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> representing the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

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

            if ((comp = this.ActivePlayer.CompareTo(state.ActivePlayer)) != 0 ||
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
        /// The Amphora <see cref="Deck"/> for the game.
        /// </summary>
        public ImmutableList<Card> Deck { get; } //TODO: Use MakeDeck to initialize the deck.)

        /// <summary>
        /// Makes the <see cref="Deck"/> used for the current number of players.
        /// </summary>
        public static ImmutableList<Card> MakeDeck(int players)
        {
            switch (players)
            {
                case 3:
                    return ImmutableList.Create(
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0));

                case 4:
                    return ImmutableList.Create(
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("Minotaur", 4),
                        new Card("Minotaur", 4),
                        new Card("Minotaur", 4),
                        new Card("Minotaur", 4),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Cerberus", 8),
                        new Card("Cerberus", 8),
                        new Card("Cerberus", 8),
                        new Card("Cerberus", 8),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0));

                case 5:
                case 6:
                    return ImmutableList.Create(
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Satyr", 1),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("Centaur", 2),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("NemeanLion", 3),
                        new Card("Minotaur", 4),
                        new Card("Minotaur", 4),
                        new Card("Minotaur", 4),
                        new Card("Minotaur", 4),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Griffin", 5),
                        new Card("Phoenix", 6),
                        new Card("Phoenix", 6),
                        new Card("Phoenix", 6),
                        new Card("Phoenix", 6),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Pegasus", 7),
                        new Card("Cerberus", 8),
                        new Card("Cerberus", 8),
                        new Card("Cerberus", 8),
                        new Card("Cerberus", 8),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Lernean Hydra", 9),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Medusa", 10),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0),
                        new Card("Charon", 0));

                default:
                    throw new InvalidOperationException();
            }
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
                    deck: value.ToImmutableList()));

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
                            state.Inventory[player].With(hand: value.ToImmutableList()))));
            }

            return shuffler.Take(maxStates);
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
            ImmutableList<Card> deck = null,
            Phase? phase = null,
            ImmutableDictionary<PlayerToken, Inventory> inventory = null)
        {
            return new GameState(
                this.Players,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                deck ?? this.Deck,
                inventory ?? this.Inventory);
        }
    }
}
