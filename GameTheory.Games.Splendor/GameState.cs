﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Splendor.
    /// </summary>
    public sealed class GameState : IGameState<Move>
    {
        /// <summary>
        /// The maxumum number of cards allowed in a player's hand.
        /// </summary>
        public const int HandLimit = 3;

        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 4;

        /// <summary>
        /// The minimum number of supported players.
        /// </summary>
        public const int MinPlayers = 2;

        /// <summary>
        /// The score at which the game ends.
        /// </summary>
        public const int ScoreLimit = 15;

        /// <summary>
        /// The maximum number of tokens a player is allowed to have.
        /// </summary>
        public const int TokenLimit = 10;

        private static readonly ImmutableList<DevelopmentCard> InitialLevel1DevelopmentCards;
        private static readonly ImmutableList<DevelopmentCard> InitialLevel2DevelopmentCards;
        private static readonly ImmutableList<DevelopmentCard> InitialLevel3DevelopmentCards;
        private static readonly ImmutableList<Noble> InitialNobles;

        static GameState()
        {
            InitialNobles = ImmutableList.Create(
                new Noble(3, 3, 3, 0, 0),
                new Noble(3, 3, 0, 0, 3),
                new Noble(3, 0, 0, 3, 3),
                new Noble(0, 0, 3, 3, 3),
                new Noble(0, 3, 3, 3, 0),
                new Noble(4, 0, 0, 0, 4),
                new Noble(4, 4, 0, 0, 0),
                new Noble(0, 4, 4, 0, 0),
                new Noble(0, 0, 4, 4, 0),
                new Noble(0, 0, 0, 4, 4));
            InitialLevel1DevelopmentCards = ImmutableList.Create(
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Onyx, 1, 1, 1, 1, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Onyx, 0, 0, 2, 1, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Onyx, 1, 2, 1, 1, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Onyx, 2, 0, 2, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Onyx, 2, 2, 0, 1, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Onyx, 0, 0, 1, 3, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Onyx, emerald: 3),
                new DevelopmentCard(DevelopmentLevel.Level1, 1, Token.Onyx, sapphire: 4),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Ruby, 1, 1, 1, 0, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Ruby, 0, 2, 1, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Ruby, 2, 1, 1, 0, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Ruby, 2, 0, 0, 2, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Ruby, 2, 0, 1, 0, 2),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Ruby, 1, 0, 0, 1, 3),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Ruby, diamond: 3),
                new DevelopmentCard(DevelopmentLevel.Level1, 1, Token.Ruby, diamond: 4),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Emerald, 1, 1, 0, 1, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Emerald, 2, 1, 0, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Emerald, 1, 1, 0, 1, 2),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Emerald, 0, 2, 0, 2, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Emerald, 0, 1, 0, 2, 2),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Emerald, 1, 3, 1, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Emerald, ruby: 3),
                new DevelopmentCard(DevelopmentLevel.Level1, 1, Token.Emerald, onyx: 4),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Sapphire, 1, 0, 1, 1, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Sapphire, 1, 0, 0, 0, 2),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Sapphire, 1, 0, 1, 2, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Sapphire, 0, 0, 2, 0, 2),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Sapphire, 1, 0, 2, 2, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Sapphire, 0, 1, 3, 1, 0),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Sapphire, onyx: 3),
                new DevelopmentCard(DevelopmentLevel.Level1, 1, Token.Sapphire, ruby: 4),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Diamond, 0, 1, 1, 1, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Diamond, 0, 0, 0, 2, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Diamond, 0, 1, 2, 1, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Diamond, 0, 2, 0, 0, 2),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Diamond, 0, 2, 2, 0, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Diamond, 3, 1, 0, 0, 1),
                new DevelopmentCard(DevelopmentLevel.Level1, 0, Token.Diamond, sapphire: 3),
                new DevelopmentCard(DevelopmentLevel.Level1, 1, Token.Diamond, emerald: 4));
            InitialLevel2DevelopmentCards = ImmutableList.Create(
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Onyx, 3, 2, 2, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Onyx, 3, 0, 3, 0, 2),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Onyx, 0, 1, 4, 2, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Onyx, 0, 0, 5, 3, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Onyx, diamond: 5),
                new DevelopmentCard(DevelopmentLevel.Level2, 3, Token.Onyx, onyx: 6),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Ruby, 2, 0, 0, 2, 3),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Ruby, 0, 3, 0, 2, 3),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Ruby, 1, 4, 2, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Ruby, 3, 0, 0, 0, 5),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Ruby, onyx: 5),
                new DevelopmentCard(DevelopmentLevel.Level2, 3, Token.Ruby, ruby: 6),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Emerald, 2, 3, 0, 0, 2),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Emerald, 3, 0, 2, 3, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Emerald, 4, 2, 0, 0, 1),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Emerald, 0, 5, 3, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Emerald, emerald: 5),
                new DevelopmentCard(DevelopmentLevel.Level2, 3, Token.Emerald, emerald: 6),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Sapphire, 0, 2, 2, 3, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Sapphire, 0, 2, 3, 0, 3),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Sapphire, 2, 0, 0, 1, 4),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Sapphire, 5, 3, 0, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Sapphire, sapphire: 5),
                new DevelopmentCard(DevelopmentLevel.Level2, 3, Token.Sapphire, sapphire: 6),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Diamond, 0, 0, 3, 2, 2),
                new DevelopmentCard(DevelopmentLevel.Level2, 1, Token.Diamond, 2, 3, 0, 3, 0),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Diamond, 0, 0, 1, 4, 2),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Diamond, 0, 0, 0, 5, 3),
                new DevelopmentCard(DevelopmentLevel.Level2, 2, Token.Diamond, ruby: 5),
                new DevelopmentCard(DevelopmentLevel.Level2, 3, Token.Diamond, diamond: 6));
            InitialLevel3DevelopmentCards = ImmutableList.Create(
                new DevelopmentCard(DevelopmentLevel.Level3, 3, Token.Onyx, 3, 3, 5, 3, 0),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Onyx, 0, 0, 3, 6, 3),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Onyx, ruby: 7),
                new DevelopmentCard(DevelopmentLevel.Level3, 5, Token.Onyx, 0, 0, 0, 7, 3),
                new DevelopmentCard(DevelopmentLevel.Level3, 3, Token.Ruby, 3, 5, 3, 0, 3),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Ruby, 0, 3, 6, 3, 0),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Ruby, emerald: 7),
                new DevelopmentCard(DevelopmentLevel.Level3, 5, Token.Ruby, 0, 0, 7, 3, 0),
                new DevelopmentCard(DevelopmentLevel.Level3, 3, Token.Emerald, 5, 3, 0, 3, 3),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Emerald, 3, 6, 3, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Emerald, sapphire: 7),
                new DevelopmentCard(DevelopmentLevel.Level3, 5, Token.Emerald, 0, 7, 3, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level3, 3, Token.Sapphire, 3, 0, 3, 3, 5),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Sapphire, 6, 3, 0, 0, 3),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Sapphire, diamond: 7),
                new DevelopmentCard(DevelopmentLevel.Level3, 5, Token.Sapphire, 7, 3, 0, 0, 0),
                new DevelopmentCard(DevelopmentLevel.Level3, 3, Token.Diamond, 0, 3, 3, 5, 3),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Diamond, 3, 0, 0, 3, 6),
                new DevelopmentCard(DevelopmentLevel.Level3, 4, Token.Diamond, onyx: 7),
                new DevelopmentCard(DevelopmentLevel.Level3, 5, Token.Diamond, 3, 0, 0, 0, 7));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
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
            this.Phase = Phase.Play;

            var gemTokens = players + (players >= 4 ? 3 : 2);
            this.Tokens = EnumCollection<Token>.Empty
                .Add(Token.Emerald, gemTokens)
                .Add(Token.Diamond, gemTokens)
                .Add(Token.Sapphire, gemTokens)
                .Add(Token.Onyx, gemTokens)
                .Add(Token.Ruby, gemTokens)
                .Add(Token.GoldJoker, 5);

            InitialNobles.Deal(players + 1, out var nobles);
            this.Nobles = nobles;

            this.Inventory = this.Players.ToImmutableDictionary(p => p, p => new Inventory());

            var developmentDecks = new[] { InitialLevel1DevelopmentCards, InitialLevel2DevelopmentCards, InitialLevel3DevelopmentCards };
            var developmentTracks = new ImmutableArray<DevelopmentCard>[developmentDecks.Length];
            for (var i = 0; i < developmentDecks.Length; i++)
            {
                var remaining = developmentDecks[i].Deal(4, out var dealt);
                developmentDecks[i] = remaining;
                developmentTracks[i] = dealt.ToImmutableArray();
            }

            this.DevelopmentDecks = developmentDecks.ToImmutableArray();
            this.DevelopmentTracks = developmentTracks.ToImmutableArray();
        }

        private GameState(
            ImmutableArray<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            EnumCollection<Token> tokens,
            ImmutableList<Noble> nobles,
            ImmutableDictionary<PlayerToken, Inventory> inventory,
            ImmutableArray<ImmutableList<DevelopmentCard>> developmentDecks,
            ImmutableArray<ImmutableArray<DevelopmentCard>> developmentTracks)
        {
            this.Players = players;
            this.ActivePlayer = activePlayer;
            this.Phase = phase;
            this.Tokens = tokens;
            this.Nobles = nobles;
            this.Inventory = inventory;
            this.DevelopmentDecks = developmentDecks;
            this.DevelopmentTracks = developmentTracks;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer { get; }

        /// <summary>
        /// Gets the decks of development cards.
        /// </summary>
        public ImmutableArray<ImmutableList<DevelopmentCard>> DevelopmentDecks { get; }

        /// <summary>
        /// Gets the visible tracks of development cards.
        /// </summary>
        public ImmutableArray<ImmutableArray<DevelopmentCard>> DevelopmentTracks { get; }

        /// <summary>
        /// Gets the inventory of all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, Inventory> Inventory { get; }

        /// <summary>
        /// Gets the remaining nobles.
        /// </summary>
        public ImmutableList<Noble> Nobles { get; }

        /// <summary>
        /// Gets the current phase of the game.
        /// </summary>
        public Phase Phase { get; }

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <summary>
        /// Gets available tokens.
        /// </summary>
        public EnumCollection<Token> Tokens { get; }

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

            if (this.DevelopmentTracks != state.DevelopmentTracks)
            {
                for (var i = 0; i < this.DevelopmentTracks.Length; i++)
                {
                    var track = this.DevelopmentTracks[i];
                    var otherTrack = state.DevelopmentTracks[i];
                    for (var c = 0; c < track.Length; c++)
                    {
                        if ((comp = track[c] == null ? -1 : track[c].CompareTo(otherTrack[c])) != 0)
                        {
                            return comp;
                        }
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
                    moves.AddRange(Moves.TakeTokensMove.GenerateMoves(this));
                    moves.AddRange(Moves.ReserveFromDeckMove.GenerateMoves(this));
                    moves.AddRange(Moves.ReserveFromBoardMove.GenerateMoves(this));
                    moves.AddRange(Moves.PurchaseFromHandMove.GenerateMoves(this));
                    moves.AddRange(Moves.PurchaseFromBoardMove.GenerateMoves(this));
                    break;

                case Phase.Discard:
                    moves.AddRange(Moves.DiscardTokensMove.GenerateMoves(this));
                    break;

                case Phase.ChooseNoble:
                    moves.AddRange(Moves.ChooseNobleMove.GenerateMoves(this));
                    break;

                case Phase.End:
                    break;
            }

            return moves.ToImmutable();
        }

        /// <summary>
        /// Gets the total bonus for the specified player.
        /// </summary>
        /// <param name="player">The player whose bonuses should be tallied.</param>
        /// <returns>The specified player's bonus.</returns>
        public EnumCollection<Token> GetBonus(PlayerToken player) =>
            new EnumCollection<Token>(this.Inventory[player].DevelopmentCards.Select(c => c.Bonus));

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
        public int GetScore(PlayerToken player) =>
            this.Inventory[player].DevelopmentCards.Sum(c => c.Prestige) + this.Inventory[player].Nobles.Count * Noble.PrestigeBonus;

        /// <inheritdoc />
        public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
        {
            var levelsInHands = new HashSet<DevelopmentLevel>();

            var shuffler = new GameShuffler<GameState>(this);

            foreach (var p in this.Players)
            {
                if (p == playerToken)
                {
                    continue;
                }

                var player = p;
                var hand = this.Inventory[player].Hand;
                for (var i = 0; i < hand.Count; i++)
                {
                    var index = i;
                    var item = hand[index];
                    levelsInHands.Add(item.Level);
                    shuffler.Add(
                        item.Level.ToString(),
                        item,
                        (state, value) => state.With(
                            inventory: state.Inventory.SetItem(
                                player,
                                state.Inventory[player].With(
                                    hand: state.Inventory[player].Hand.SetItem(index, value)))));
                }
            }

            if (levelsInHands.Count == 0)
            {
                return new[] { this };
            }
            else
            {
                foreach (var level in levelsInHands)
                {
                    var levelIndex = (int)level;
                    shuffler.AddCollection(
                        level.ToString(),
                        this.DevelopmentDecks[levelIndex],
                        (state, value) => state.With(
                            developmentDecks: state.DevelopmentDecks.SetItem(
                                levelIndex,
                                value.ToImmutableList())));
                }

                return shuffler.Take(maxStates);
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> GetWinners()
        {
            if (this.Phase != Phase.End)
            {
                return ImmutableList<PlayerToken>.Empty;
            }

            return this.Players
                .GroupBy(p => this.GetScore(p))
                .OrderByDescending(g => g.Key)
                .First()
                .GroupBy(p => this.Inventory[p].DevelopmentCards.Count)
                .OrderBy(g => g.Key)
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

        internal GameState With(
            PlayerToken activePlayer = null,
            Phase? phase = null,
            EnumCollection<Token> tokens = null,
            ImmutableList<Noble> nobles = null,
            ImmutableDictionary<PlayerToken, Inventory> inventory = null,
            ImmutableArray<ImmutableList<DevelopmentCard>>? developmentDecks = null,
            ImmutableArray<ImmutableArray<DevelopmentCard>>? developmentTracks = null)
        {
            return new GameState(
                this.Players,
                activePlayer ?? this.ActivePlayer,
                phase ?? this.Phase,
                tokens ?? this.Tokens,
                nobles ?? this.Nobles,
                inventory ?? this.Inventory,
                developmentDecks ?? this.DevelopmentDecks,
                developmentTracks ?? this.DevelopmentTracks);
        }
    }
}
