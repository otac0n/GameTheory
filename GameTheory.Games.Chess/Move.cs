// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess
{
    using System.Collections.Generic;
    using System.Linq;

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

        /// <summary>
        /// Gets a value indicating whether or not move results in check.
        /// </summary>
        public bool IsCheck => this.Apply(this.GameState).IsCheck;

        /// <summary>
        /// Gets a value indicating whether or not move results in checkmate.
        /// </summary>
        public bool IsCheckmate => this.Apply(this.GameState).IsCheckmate;

        /// <inheritdoc/>
        public bool IsDeterministic => true;

        /// <inheritdoc/>
        public PlayerToken PlayerToken { get; }

        internal GameState GameState { get; }

        /// <inheritdoc />
        public sealed override string ToString() => $"{this.GameState.MoveNumber}.{(this.GameState.ActiveColor == Pieces.Black ? ".." : string.Empty)} {string.Concat(this.FlattenFormatTokens().Select(t => t is Pieces piece ? this.GameState.Variant.NotationSystem.Format(piece) : t))}{(this.IsCheckmate ? "#" : this.IsCheck ? "+" : string.Empty)}";

        internal abstract GameState Apply(GameState state);
    }
}
