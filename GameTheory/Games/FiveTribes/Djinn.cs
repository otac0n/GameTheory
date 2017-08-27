// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// The base class for all Djinns.
    /// </summary>
    public abstract class Djinn : IComparable<Djinn>, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Djinn"/> class.
        /// </summary>
        /// <param name="value">The value of the <see cref="Djinn"/>, in victory points (VP).</param>
        protected Djinn(int value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the name of the Djinn.
        /// </summary>
        public virtual string Name => this.GetType().Name;

        /// <summary>
        /// Gets the value of the Djinn, in victory points (VP).
        /// </summary>
        public int Value { get; }

        /// <inheritdoc />
        public IList<object> FormatTokens => new object[] { this.Name, " +", this.Value };

        /// <summary>
        /// Called for every <see cref="GameState"/>, to allow inclusion of additional <see cref="Move">Moves</see>.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> for which additional <see cref="Move">Moves</see> are being generated.</param>
        /// <param name="moves">The standard <see cref="Move">Moves</see> returned by the <see cref="GameState"/>.</param>
        /// <returns>The additional <see cref="Move">Moves</see>.</returns>
        public virtual IEnumerable<Move> GetAdditionalMoves(GameState state, IList<Move> moves)
        {
            return ImmutableList<Move>.Empty;
        }

        /// <summary>
        /// Generate moves for any <see cref="GameState"/> where this <see cref="Djinn"/> is owned by the active player.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> for which <see cref="Move">Moves</see> are being generated.</param>
        /// <returns>The <see cref="Move">Moves</see> provided by the <see cref="Djinn"/>.</returns>
        public virtual IEnumerable<Move> GetMoves(GameState state)
        {
            return ImmutableList<Move>.Empty;
        }

        /// <summary>
        /// Handle an assassination in front of a player.
        /// </summary>
        /// <param name="owner">The owner of the <see cref="Djinn"/>.</param>
        /// <param name="state">The <see cref="GameState"/> that has recently seen an assassination.</param>
        /// <param name="victim">The victim of the assassination.</param>
        /// <param name="kill">The <see cref="Meeple">Meeples</see> killed.</param>
        /// <returns>An updated <see cref="GameState"/>.</returns>
        public virtual GameState HandleAssassination(PlayerToken owner, GameState state, PlayerToken victim, EnumCollection<Meeple> kill)
        {
            return state;
        }

        /// <summary>
        /// Handle an assassination in the <see cref="Sultanate"/>.
        /// </summary>
        /// <param name="owner">The owner of the <see cref="Djinn"/>.</param>
        /// <param name="state">The <see cref="GameState"/> that has recently seen an assassination.</param>
        /// <param name="point">The location of the assassination.</param>
        /// <param name="kill">The <see cref="Meeple">Meeples</see> killed.</param>
        /// <returns>An updated <see cref="GameState"/>.</returns>
        public virtual GameState HandleAssassination(PlayerToken owner, GameState state, Point point, EnumCollection<Meeple> kill)
        {
            return state;
        }

        /// <summary>
        /// Handle the transition between states.
        /// </summary>
        /// <param name="owner">The owner of the <see cref="Djinn"/>.</param>
        /// <param name="oldState">The oldest <see cref="GameState"/> that has been handled by this Djinn.</param>
        /// <param name="newState">The newest <see cref="GameState"/>.</param>
        /// <returns>An updated <see cref="GameState"/>.</returns>
        public virtual GameState HandleTransition(PlayerToken owner, GameState oldState, GameState newState)
        {
            return newState;
        }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        /// <inheritdoc/>
        public int CompareTo(Djinn other)
        {
            if (other == this)
            {
                return 0;
            }
            else if (other == null)
            {
                return 1;
            }

            return string.Compare(this.GetType().Name, other.GetType().Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
