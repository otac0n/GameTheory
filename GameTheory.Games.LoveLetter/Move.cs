// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Love Letter.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="LoveLetter.GameState"/> that this move is based on.</param>
        public Move(GameState state)
        {
            this.GameState = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = state.ActivePlayer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="LoveLetter.GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        protected Move(GameState state, PlayerToken player)
        {
            this.GameState = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = player;
        }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public virtual bool IsDeterministic => true;

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
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            if (state.Phase == Phase.Draw)
            {
                state = state.With(
                    phase: Phase.Discard);
            }
            else if (state.Phase == Phase.Discard)
            {
                var nextPlayer = activePlayer;
                if (state.Deck.Count > 0 && state.Inventory.Values.Where(inv => inv.Hand.Length > 0).Take(2).Count() > 1)
                {
                    do
                    {
                        nextPlayer = state.GetNextPlayer(nextPlayer);
                    }
                    while (nextPlayer != activePlayer && state.Inventory[nextPlayer].Hand.Length == 0);
                }

                if (nextPlayer == activePlayer)
                {
                    state = state.With(
                        phase: Phase.Reveal);
                }
                else
                {
                    state = state.With(
                        activePlayer: nextPlayer,
                        phase: Phase.Draw);
                }
            }

            return state;
        }
    }
}
