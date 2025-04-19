// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Seven Dragons.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="FiveTribes.GameState"/> that this move is based on.</param>
        protected Move(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            this.GameState = state;
            this.PlayerToken = state.ActivePlayer;
        }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public bool IsDeterministic => true;

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
            if (state.Phase == Phase.Draw)
            {
                state = state.With(
                    phase: Phase.Play);
            }
            else if (state.Phase == Phase.Play)
            {
                var connections = state.GetConnections();
                if (connections.Any(goal => goal.Value.Any(group => new HashSet<Point>(group.Select(g => g.Item1)).Count >= GameState.WinningGroupSize)))
                {
                    state = state.With(
                        phase: Phase.End);
                }
                else
                {
                    state = state.With(
                        phase: Phase.Draw,
                        activePlayer: state.Players.GetNextPlayer(state.ActivePlayer));
                }
            }

            return state;
        }
    }
}
