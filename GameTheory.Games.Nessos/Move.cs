// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a move in Nessos.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="Nessos.GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        protected Move(GameState state, PlayerToken player)
        {
            this.GameState = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = player;
        }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        internal GameState GameState { get; }

        /// <inheritdoc />
        public virtual int CompareTo(Move other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            return string.Compare(this.GetType().Name, other.GetType().Name, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            var activePlayer = state.FirstPlayer;

            if (state.Phase == Phase.Draw)
            {
                // TODO: Exclude players with 3 charons.
                if (state.Deck.Count == 0 || state.Inventory.All(i => i.Value.Hand.Count >= GameState.HandLimit))
                {
                    state = state.With(
                        firstPlayer: state.GetNextPlayer(state.FirstPlayer),
                        phase: Phase.Offer);
                }
            }


            return state;
        }
    }
}
