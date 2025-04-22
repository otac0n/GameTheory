// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Century Spice Road.
    /// </summary>
    public abstract class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="CenturySpiceRoad.GameState"/> that this move is based on.</param>
        protected Move(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            this.GameState = state;
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
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            if (Moves.DiscardSpicesMove.ShouldTransitionToPhase(state))
            {
                return state.With(
                    phase: Phase.Discard);
            }

            if (state.Phase == Phase.Play)
            {
                state = state.With(
                    activePlayer: state.Players.GetNextPlayer(state.ActivePlayer));
            }

            if (state.ActivePlayer == state.Players[0] && state.Players.Any(p => state.Inventory[p].PointCards.Count >= GameState.PointCardLimit))
            {
                state = state.With(
                    phase: Phase.End);
            }

            return state;
        }

        internal virtual IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state) => throw new NotImplementedException();
    }
}
