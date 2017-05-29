// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Represents the current state of a game of Splendor.
    /// </summary>
    public class GameState : IGameState<Move>
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

        private readonly PlayerToken activePlayer;
        private readonly ImmutableArray<ImmutableList<DevelopmentCard>> developmentDecks;
        private readonly ImmutableArray<ImmutableArray<DevelopmentCard>> developmentTracks;
        private readonly ImmutableDictionary<PlayerToken, Inventory> inventory;
        private readonly ImmutableList<Noble> nobles;
        private readonly Phase phase;
        private readonly ImmutableList<PlayerToken> players;
        private readonly EnumCollection<Token> tokens;
        private readonly Func<GameState, IEnumerable<Move>> subsequentMovesFactory;
        private readonly Lazy<ImmutableList<Move>> subsequentMoves;

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
                new DevelopmentCard(0, Token.Onyx, 1, 1, 1, 1, 0),
                new DevelopmentCard(0, Token.Onyx, 0, 0, 2, 1, 0),
                new DevelopmentCard(0, Token.Onyx, 1, 2, 1, 1, 0),
                new DevelopmentCard(0, Token.Onyx, 2, 0, 2, 0, 0),
                new DevelopmentCard(0, Token.Onyx, 2, 2, 0, 1, 0),
                new DevelopmentCard(0, Token.Onyx, 0, 0, 1, 3, 1),
                new DevelopmentCard(0, Token.Onyx, emerald: 3),
                new DevelopmentCard(1, Token.Onyx, sapphire: 4),
                new DevelopmentCard(0, Token.Ruby, 1, 1, 1, 0, 1),
                new DevelopmentCard(0, Token.Ruby, 0, 2, 1, 0, 0),
                new DevelopmentCard(0, Token.Ruby, 2, 1, 1, 0, 1),
                new DevelopmentCard(0, Token.Ruby, 2, 0, 0, 2, 0),
                new DevelopmentCard(0, Token.Ruby, 2, 0, 1, 0, 2),
                new DevelopmentCard(0, Token.Ruby, 1, 0, 0, 1, 3),
                new DevelopmentCard(0, Token.Ruby, diamond: 3),
                new DevelopmentCard(1, Token.Ruby, diamond: 4),
                new DevelopmentCard(0, Token.Emerald, 1, 1, 0, 1, 1),
                new DevelopmentCard(0, Token.Emerald, 2, 1, 0, 0, 0),
                new DevelopmentCard(0, Token.Emerald, 1, 1, 0, 1, 2),
                new DevelopmentCard(0, Token.Emerald, 0, 2, 0, 2, 0),
                new DevelopmentCard(0, Token.Emerald, 0, 1, 0, 2, 2),
                new DevelopmentCard(0, Token.Emerald, 1, 3, 1, 0, 0),
                new DevelopmentCard(0, Token.Emerald, ruby: 3),
                new DevelopmentCard(1, Token.Emerald, onyx: 4),
                new DevelopmentCard(0, Token.Sapphire, 1, 0, 1, 1, 1),
                new DevelopmentCard(0, Token.Sapphire, 1, 0, 0, 0, 2),
                new DevelopmentCard(0, Token.Sapphire, 1, 0, 1, 2, 1),
                new DevelopmentCard(0, Token.Sapphire, 0, 0, 2, 0, 2),
                new DevelopmentCard(0, Token.Sapphire, 1, 0, 2, 2, 0),
                new DevelopmentCard(0, Token.Sapphire, 0, 1, 3, 1, 0),
                new DevelopmentCard(0, Token.Sapphire, onyx: 3),
                new DevelopmentCard(1, Token.Sapphire, ruby: 4),
                new DevelopmentCard(0, Token.Diamond, 0, 1, 1, 1, 1),
                new DevelopmentCard(0, Token.Diamond, 0, 0, 0, 2, 1),
                new DevelopmentCard(0, Token.Diamond, 0, 1, 2, 1, 1),
                new DevelopmentCard(0, Token.Diamond, 0, 2, 0, 0, 2),
                new DevelopmentCard(0, Token.Diamond, 0, 2, 2, 0, 1),
                new DevelopmentCard(0, Token.Diamond, 3, 1, 0, 0, 1),
                new DevelopmentCard(0, Token.Diamond, sapphire: 3),
                new DevelopmentCard(1, Token.Diamond, emerald: 4));
            InitialLevel2DevelopmentCards = ImmutableList.Create(
                new DevelopmentCard(1, Token.Onyx, 3, 2, 2, 0, 0),
                new DevelopmentCard(1, Token.Onyx, 3, 0, 3, 0, 2),
                new DevelopmentCard(2, Token.Onyx, 0, 1, 4, 2, 0),
                new DevelopmentCard(2, Token.Onyx, 0, 0, 5, 3, 0),
                new DevelopmentCard(2, Token.Onyx, diamond: 5),
                new DevelopmentCard(3, Token.Onyx, onyx: 6),
                new DevelopmentCard(1, Token.Ruby, 2, 0, 0, 2, 3),
                new DevelopmentCard(1, Token.Ruby, 0, 3, 0, 2, 3),
                new DevelopmentCard(2, Token.Ruby, 1, 4, 2, 0, 0),
                new DevelopmentCard(2, Token.Ruby, 3, 0, 0, 0, 5),
                new DevelopmentCard(2, Token.Ruby, onyx: 5),
                new DevelopmentCard(3, Token.Ruby, ruby: 6),
                new DevelopmentCard(1, Token.Emerald, 2, 3, 0, 0, 2),
                new DevelopmentCard(1, Token.Emerald, 3, 0, 2, 3, 0),
                new DevelopmentCard(2, Token.Emerald, 4, 2, 0, 0, 1),
                new DevelopmentCard(2, Token.Emerald, 0, 5, 3, 0, 0),
                new DevelopmentCard(2, Token.Emerald, emerald: 5),
                new DevelopmentCard(3, Token.Emerald, emerald: 6),
                new DevelopmentCard(1, Token.Sapphire, 0, 2, 2, 3, 0),
                new DevelopmentCard(1, Token.Sapphire, 0, 2, 3, 0, 3),
                new DevelopmentCard(2, Token.Sapphire, 2, 0, 0, 1, 4),
                new DevelopmentCard(2, Token.Sapphire, 5, 3, 0, 0, 0),
                new DevelopmentCard(2, Token.Sapphire, sapphire: 5),
                new DevelopmentCard(3, Token.Sapphire, sapphire: 6),
                new DevelopmentCard(1, Token.Diamond, 0, 0, 3, 2, 2),
                new DevelopmentCard(1, Token.Diamond, 2, 3, 0, 3, 0),
                new DevelopmentCard(2, Token.Diamond, 0, 0, 1, 4, 2),
                new DevelopmentCard(2, Token.Diamond, 0, 0, 0, 5, 3),
                new DevelopmentCard(2, Token.Diamond, ruby: 5),
                new DevelopmentCard(3, Token.Diamond, diamond: 6));
            InitialLevel3DevelopmentCards = ImmutableList.Create(
                new DevelopmentCard(3, Token.Onyx, 3, 3, 5, 3, 0),
                new DevelopmentCard(4, Token.Onyx, 0, 0, 3, 6, 3),
                new DevelopmentCard(4, Token.Onyx, ruby: 7),
                new DevelopmentCard(5, Token.Onyx, 0, 0, 0, 7, 3),
                new DevelopmentCard(3, Token.Ruby, 3, 5, 3, 0, 3),
                new DevelopmentCard(4, Token.Ruby, 0, 3, 6, 3, 0),
                new DevelopmentCard(4, Token.Ruby, emerald: 7),
                new DevelopmentCard(5, Token.Ruby, 0, 0, 7, 3, 0),
                new DevelopmentCard(3, Token.Emerald, 5, 3, 0, 3, 3),
                new DevelopmentCard(4, Token.Emerald, 3, 6, 3, 0, 0),
                new DevelopmentCard(4, Token.Emerald, sapphire: 7),
                new DevelopmentCard(5, Token.Emerald, 0, 7, 3, 0, 0),
                new DevelopmentCard(3, Token.Sapphire, 3, 0, 3, 3, 5),
                new DevelopmentCard(4, Token.Sapphire, 6, 3, 0, 0, 3),
                new DevelopmentCard(4, Token.Sapphire, diamond: 7),
                new DevelopmentCard(5, Token.Sapphire, 7, 3, 0, 0, 0),
                new DevelopmentCard(3, Token.Diamond, 0, 3, 3, 5, 3),
                new DevelopmentCard(4, Token.Diamond, 3, 0, 0, 3, 6),
                new DevelopmentCard(4, Token.Diamond, onyx: 7),
                new DevelopmentCard(5, Token.Diamond, 3, 0, 0, 0, 7));
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

            this.players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableList();
            this.activePlayer = this.players[0];
            this.phase = Phase.Play;

            var gemTokens = players + (players >= 4 ? 3 : 2);
            this.tokens = EnumCollection<Token>.Empty
                .Add(Token.Emerald, gemTokens)
                .Add(Token.Diamond, gemTokens)
                .Add(Token.Sapphire, gemTokens)
                .Add(Token.Onyx, gemTokens)
                .Add(Token.Ruby, gemTokens)
                .Add(Token.GoldJoker, 5);

            InitialNobles.Deal(players + 1, out this.nobles);

            this.inventory = this.players.ToImmutableDictionary(p => p, p => new Inventory());

            var developmentDecks = new[] { InitialLevel1DevelopmentCards, InitialLevel2DevelopmentCards, InitialLevel3DevelopmentCards };
            var developmentTracks = new ImmutableArray<DevelopmentCard>[developmentDecks.Length];
            for (var i = 0; i < developmentDecks.Length; i++)
            {
                var remaining = developmentDecks[i].Deal(4, out ImmutableList<DevelopmentCard> dealt);
                developmentDecks[i] = remaining;
                developmentTracks[i] = dealt.ToImmutableArray();
            }

            this.developmentDecks = developmentDecks.ToImmutableArray();
            this.developmentTracks = developmentTracks.ToImmutableArray();
        }

        private GameState(
            ImmutableList<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            EnumCollection<Token> tokens,
            ImmutableList<Noble> nobles,
            ImmutableDictionary<PlayerToken, Inventory> inventory,
            ImmutableArray<ImmutableList<DevelopmentCard>> developmentDecks,
            ImmutableArray<ImmutableArray<DevelopmentCard>> developmentTracks,
            Func<GameState, IEnumerable<Move>> subsequentMovesFactory)
            : this(subsequentMovesFactory)
        {
            this.players = players;
            this.activePlayer = activePlayer;
            this.phase = phase;
            this.tokens = tokens;
            this.nobles = nobles;
            this.inventory = inventory;
            this.developmentDecks = developmentDecks;
            this.developmentTracks = developmentTracks;
        }

        private GameState(Func<GameState, IEnumerable<Move>> subsequentMovesFactory)
        {
            this.subsequentMovesFactory = subsequentMovesFactory;
            this.subsequentMoves = subsequentMovesFactory == null ? null : new Lazy<ImmutableList<Move>>(() => this.subsequentMovesFactory(this).ToImmutableList(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer => this.activePlayer;

        /// <summary>
        /// Gets the decks of development cards.
        /// </summary>
        public ImmutableArray<ImmutableList<DevelopmentCard>> DevelopmentDecks => this.developmentDecks;

        /// <summary>
        /// Gets the visible tracks of development cards.
        /// </summary>
        public ImmutableArray<ImmutableArray<DevelopmentCard>> DevelopmentTracks => this.developmentTracks;

        /// <summary>
        /// Gets the inventory of all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, Inventory> Inventory => this.inventory;

        /// <summary>
        /// Gets the remaining nobles.
        /// </summary>
        public ImmutableList<Noble> Nobles => this.nobles;

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players => this.players;

        /// <summary>
        /// Gets available tokens.
        /// </summary>
        public EnumCollection<Token> Tokens => this.tokens;

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves()
        {
            if (this.subsequentMovesFactory != null)
            {
                return this.subsequentMoves.Value;
            }
            else
            {
                var moves = ImmutableList.CreateBuilder<Move>();

                if (this.phase != Phase.End)
                {
                    moves.AddRange(Moves.TakeTokensMove.GenerateMoves(this));
                    moves.AddRange(Moves.ReserveFromDeckMove.GenerateMoves(this));
                    moves.AddRange(Moves.ReserveFromBoardMove.GenerateMoves(this));
                    moves.AddRange(Moves.PurchaseFromHandMove.GenerateMoves(this));
                    moves.AddRange(Moves.PurchaseFromBoardMove.GenerateMoves(this));
                }

                return moves.ToImmutable();
            }
        }

        /// <summary>
        /// Gets the total bonus for the specified player.
        /// </summary>
        /// <param name="player">The player whose bonuses should be tallied.</param>
        /// <returns>The specified player's bonus.</returns>
        public EnumCollection<Token> GetBonus(PlayerToken player) =>
            new EnumCollection<Token>(this.Inventory[player].DevelopmentCards.Select(c => c.Bonus));

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="player">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken player) =>
            this.Inventory[player].DevelopmentCards.Sum(c => c.Prestige) + this.Inventory[player].Nobles.Count * Noble.PrestigeBonus;

        /// <inheritdoc />
        public IGameState<Move> GetView(PlayerToken playerToken) => this;

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.phase == Phase.Play)
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
                this.players,
                activePlayer ?? this.activePlayer,
                phase ?? this.phase,
                tokens ?? this.tokens,
                nobles ?? this.nobles,
                inventory ?? this.inventory,
                developmentDecks ?? this.developmentDecks,
                developmentTracks ?? this.developmentTracks,
                null);
        }

        internal GameState WithMoves(Func<GameState, Move> subsequentMoves)
        {
            return this.WithMoves(s => new[] { subsequentMoves(s) });
        }

        internal GameState WithMoves(Func<GameState, IEnumerable<Move>> subsequentMoves)
        {
            return new GameState(
                this.players,
                this.activePlayer,
                this.phase,
                this.tokens,
                this.nobles,
                this.inventory,
                this.developmentDecks,
                this.developmentTracks,
                subsequentMoves);
        }
    }
}
