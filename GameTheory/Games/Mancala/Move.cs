// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a move in Mancala.
    /// </summary>
    public sealed class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="bin">The index of the bin.</param>
        internal Move(GameState state, int bin)
        {
            Contract.Requires(state != null);

            this.State = state;
            this.Player = state.ActivePlayer;
            this.Bin = bin;
        }

        /// <summary>
        /// Gets the index of the bin.
        /// </summary>
        public int Bin { get; private set; }

        /// <summary>
        /// Gets the player who may perform this move.
        /// </summary>
        public PlayerToken Player { get; private set; }

        internal GameState State { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Pick up {this.Bin}";
        }

        internal GameState Apply(GameState state)
        {
            return state;
        }
    }
}
