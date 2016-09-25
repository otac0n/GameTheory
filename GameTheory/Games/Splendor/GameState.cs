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

        private static readonly EnumCollection<Token> InititalTokens;

        private readonly int activePlayer;
        private readonly Phase phase;
        private readonly ImmutableList<PlayerToken> players;

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
            this.activePlayer = 0;
            this.phase = Phase.Play;
        }

        /// <summary>
        /// Gets the active player.
        /// </summary>
        public PlayerToken ActivePlayer => this.players[this.activePlayer];

        IReadOnlyList<PlayerToken> IGameState<Move>.Players => this.Players;

        /// <summary>
        /// Gets the list of players.
        /// </summary>
        public ImmutableList<PlayerToken> Players => this.players;

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken player)
        {
            var moves = new List<Move>();

            return moves.ToImmutableList();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            return ImmutableList<PlayerToken>.Empty;
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
    }
}
