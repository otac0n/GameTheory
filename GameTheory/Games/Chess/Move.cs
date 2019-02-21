// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move in <see cref="Chess.GameState">Chess</see>.
    /// </summary>
    public abstract class Move : IMove
    {
        internal Move(GameState state)
        {
            this.GameState = state;
            this.PlayerToken = state.ActivePlayer;
        }

        /// <inheritdoc/>
        public virtual IList<object> FormatTokens => this.GameState.Variant.NotationSystem.Format(this);

        /// <inheritdoc/>
        public bool IsDeterministic => true;

        /// <inheritdoc/>
        public PlayerToken PlayerToken { get; }

        internal GameState GameState { get; }

        /// <summary>
        /// Advances the game to the next player and the next phase if necessary.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> to advance.</param>
        /// <returns>The updated state with the next player active.</returns>
        public static GameState Advance(GameState state)
        {
            return state.With(
                enPassantIndex: state.EnPassantIndex, // maintain en passant
                activeColor: state.ActiveColor == Pieces.White ? Pieces.Black : Pieces.White);
        }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal abstract GameState Apply(GameState state);
    }
}
