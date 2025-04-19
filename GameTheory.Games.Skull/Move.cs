// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Skull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Skull.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="Skull.GameState"/> that this move is based on.</param>
        protected Move(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            this.GameState = state;
            this.PlayerToken = state.ActivePlayer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="Skull.GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        protected Move(GameState state, PlayerToken player)
        {
            ArgumentNullException.ThrowIfNull(state);

            this.GameState = state;
            this.PlayerToken = player;
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
            var activePlayer = state.ActivePlayer;

            var phase = state.Phase;
            if (phase == Phase.Bidding)
            {
                var count = 0;
                var startingIndex = state.Players.IndexOf(activePlayer);
                var activePlayerSet = false;
                var i = startingIndex;
                while (true)
                {
                    var player = state.Players[i];
                    if (state.Inventory[player].Bid > Inventory.PassingBid)
                    {
                        count++;
                        if (!activePlayerSet && i != startingIndex)
                        {
                            activePlayer = player;
                            activePlayerSet = true;
                        }
                    }

                    i = (i + 1) % state.Players.Length;
                    if (i == startingIndex)
                    {
                        break;
                    }
                }

                state = state.With(
                    phase: count == 1 ? Phase.Challenge : Phase.Bidding,
                    activePlayer: activePlayer);
            }
            else if (phase == Phase.AddingCards)
            {
                if (state.Inventory.All(i => i.Value.Bid <= Inventory.PassingBid || i.Value.Stack.Count == 1))
                {
                    // The starting player moves twice in a row.
                }
                else
                {
                    var startingIndex = state.Players.IndexOf(activePlayer);
                    var i = startingIndex;
                    while (true)
                    {
                        i = (i + 1) % state.Players.Length;
                        activePlayer = state.Players[i];
                        if (i == startingIndex || state.Inventory[activePlayer].Bid > Inventory.PassingBid)
                        {
                            break;
                        }
                    }

                    state = state.With(
                        activePlayer: activePlayer);
                }
            }

            return state;
        }
    }
}
