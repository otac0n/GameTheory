// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents the current state of a game of Splendor.
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
        /// The score at which the game ends.
        /// </summary>
        public const int ScoreLimit = 15;

        private static readonly EnumCollection<Token> InititalTokens;

        private readonly PlayerToken activePlayer;
        private readonly Phase phase;
        private readonly ImmutableList<PlayerToken> players;
        private readonly ImmutableDictionary<PlayerToken, EnumCollection<Token>> playerTokens;
        private readonly EnumCollection<Token> tokens;

        static GameState()
        {
            InititalTokens = new EnumCollection<Token>()
                .Add(Token.Emerald, 7)
                .Add(Token.Diamond, 7)
                .Add(Token.Sapphire, 7)
                .Add(Token.Onyx, 7)
                .Add(Token.Ruby, 7)
                .Add(Token.GoldJoker, 5);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="players">The number of players.</param>
        public GameState(int players)
        {
            Contract.Requires(players >= MinPlayers && players <= MaxPlayers);
            this.players = Enumerable.Range(0, players).Select(i => new PlayerToken()).ToImmutableList();
            this.activePlayer = this.players[0];
            this.phase = Phase.Play;
            this.tokens = InititalTokens;
            this.playerTokens = this.players.ToImmutableDictionary(p => p, p => new EnumCollection<Token>());
        }

        public GameState(
            ImmutableList<PlayerToken> players,
            PlayerToken activePlayer,
            Phase phase,
            EnumCollection<Token> tokens,
            ImmutableDictionary<PlayerToken, EnumCollection<Token>> playerTokens)
        {
            this.players = players;
            this.activePlayer = activePlayer;
            this.phase = phase;
            this.tokens = tokens;
            this.playerTokens = playerTokens;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer => this.activePlayer;

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players => this.players;

        /// <summary>
        /// Gets the tokens owned by all players.
        /// </summary>
        public ImmutableDictionary<PlayerToken, EnumCollection<Token>> PlayerTokens => this.playerTokens;

        /// <summary>
        /// Gets available tokens.
        /// </summary>
        public EnumCollection<Token> Tokens => this.tokens;

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken player)
        {
            var moves = new List<Move>();

            if (this.phase != Phase.End)
            {
            }

            return moves.ToImmutableList();
        }

        /// <summary>
        /// Gets the score of the specified player.
        /// </summary>
        /// <param name="player">The player whose score should be calculated.</param>
        /// <returns>The specified player's score.</returns>
        public int GetScore(PlayerToken player)
        {
            return 0;
        }

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
            Contract.Requires(move != null);
            Contract.Requires(move.State == this);

            return move.Apply(this);
        }

        internal GameState With(
            PlayerToken activePlayer = null,
            Phase? phase = null,
            EnumCollection<Token> tokens = null,
            ImmutableDictionary<PlayerToken, EnumCollection<Token>> playerTokens = null)
        {
            return new GameState(
                this.players,
                activePlayer ?? this.activePlayer,
                phase ?? this.phase,
                tokens ?? this.tokens,
                playerTokens ?? this.playerTokens);
        }
    }
}
