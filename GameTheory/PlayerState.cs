// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    /// <summary>
    /// A tuple of a player token and a game state.
    /// </summary>
    /// <typeparam name="TMove">The type of move in the game state.</typeparam>
    public struct PlayerState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerState{TMove}"/> struct.
        /// </summary>
        /// <param name="playerToken">The player token.</param>
        /// <param name="state">The current state of the game.</param>
        public PlayerState(PlayerToken playerToken, IGameState<TMove> state)
        {
            this.PlayerToken = playerToken;
            this.GameState = state;
        }

        /// <summary>
        /// Gets the current state of the game.
        /// </summary>
        public IGameState<TMove> GameState { get; }

        /// <summary>
        /// Gets the player token.
        /// </summary>
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Compares two <see cref="PlayerState{TMove}"/> objects. The result specifies whether they are unequal.
        /// </summary>
        /// <param name="left">The first <see cref="PlayerState{TMove}"/> to compare.</param>
        /// <param name="right">The second <see cref="PlayerState{TMove}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> differ; otherwise, <c>false</c>.</returns>
        public static bool operator !=(PlayerState<TMove> left, PlayerState<TMove> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compares two <see cref="PlayerState{TMove}"/> objects. The result specifies whether they are equal.
        /// </summary>
        /// <param name="left">The first <see cref="PlayerState{TMove}"/> to compare.</param>
        /// <param name="right">The second <see cref="PlayerState{TMove}"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(PlayerState<TMove> left, PlayerState<TMove> right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is PlayerState<TMove> other)
            {
                return this.Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> and this instance represent the same value; otherwise, <c>false</c>.</returns>
        public bool Equals(PlayerState<TMove> other) =>
            this.GameState == other.GameState &&
            this.PlayerToken == other.PlayerToken;

        /// <inheritdoc/>
        public override int GetHashCode() =>
            this.GameState.GetHashCode() ^ this.PlayerToken.GetHashCode();
    }
}
