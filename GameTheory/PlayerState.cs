// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;

    /// <summary>
    /// A tuple of a player token and a game state.
    /// </summary>
    /// <typeparam name="TGameState">The type of game state.</typeparam>
    /// <typeparam name="TMove">The type of move in the game state.</typeparam>
    public struct PlayerState<TGameState, TMove> : IEquatable<PlayerState<TGameState, TMove>>
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerState{TGameState, TMove}"/> struct.
        /// </summary>
        /// <param name="playerToken">The player token.</param>
        /// <param name="state">The current state of the game.</param>
        public PlayerState(PlayerToken playerToken, TGameState state)
        {
            this.PlayerToken = playerToken;
            this.GameState = state;
        }

        /// <summary>
        /// Gets the current state of the game.
        /// </summary>
        public TGameState GameState { get; }

        /// <summary>
        /// Gets the player token.
        /// </summary>
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Compares two <see cref="PlayerState{TGameState, TMove}"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="left">The first <see cref="PlayerState{TGameState, TMove}"/> to compare.</param>
        /// <param name="right">The second <see cref="PlayerState{TGameState, TMove}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(PlayerState<TGameState, TMove> left, PlayerState<TGameState, TMove> right) => !(left == right);

        /// <summary>
        /// Compares two <see cref="PlayerState{TGameState, TMove}"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="left">The first <see cref="PlayerState{TGameState, TMove}"/> to compare.</param>
        /// <param name="right">The second <see cref="PlayerState{TGameState, TMove}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(PlayerState<TGameState, TMove> left, PlayerState<TGameState, TMove> right) => left.Equals(right);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is PlayerState<TGameState, TMove> other && this.Equals(other);

        /// <inheritdoc/>
        public bool Equals(PlayerState<TGameState, TMove> other) =>
            (object.ReferenceEquals(this.GameState, other.GameState) || (this.GameState != null && this.GameState.Equals(other.GameState))) &&
            this.PlayerToken == other.PlayerToken;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = HashUtilities.Seed;
            HashUtilities.Combine(ref hash, this.GameState.GetHashCode());
            HashUtilities.Combine(ref hash, this.PlayerToken.GetHashCode());
            return hash;
        }
    }
}
