// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PositivelyPerfectParfaitGame
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Positively Perfect Parfait Game.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="PositivelyPerfectParfaitGame.GameState"/> that this move is based on.</param>
        protected Move(GameState state)
        {
            this.GameState = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = state.ActivePlayer;
        }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public abstract bool IsDeterministic { get; }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
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
            if (state.Phase == Phase.Play)
            {
                if (state.Parfaits[state.ActivePlayer].Cherry && (!state.PlayOut || state.Players.All(p => state.Parfaits[p].Cherry)))
                {
                    state = state.With(
                        phase: Phase.End);
                }
                else
                {
                    do
                    {
                        state = state.With(
                            activePlayer: state.Players.GetNextPlayer(state.ActivePlayer));
                    }
                    while (state.Parfaits[state.ActivePlayer].Cherry);
                }
            }

            return state;
        }

        internal virtual IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state) => throw new NotImplementedException();
    }
}
