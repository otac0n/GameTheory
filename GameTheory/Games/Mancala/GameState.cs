// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents the current state of a game of Mancala.
    /// </summary>
    public class GameState : IGameState<Move>
    {
        private readonly PlayerToken activePlayer;
        private readonly ImmutableList<PlayerToken> players;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        public GameState()
        {
            this.players = ImmutableList.Create(new PlayerToken(), new PlayerToken());
            this.activePlayer = this.players[0];
        }

        private GameState(
            PlayerToken activePlayer)
        {
            this.activePlayer = activePlayer;
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

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken player)
        {
            var moves = new List<Move>();

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
            PlayerToken activePlayer = null)
        {
            return new GameState(
                activePlayer ?? this.activePlayer);
        }
    }
}
