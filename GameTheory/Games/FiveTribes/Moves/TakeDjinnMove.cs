// -----------------------------------------------------------------------
// <copyright file="TakeDjinnMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move to take a specified <see cref="Djinn"/>.
    /// </summary>
    public class TakeDjinnMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeDjinnMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Djinn"/> that will be taken.</param>
        public TakeDjinnMove(GameState state0, int index)
            : this(state0, index, s => s)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeDjinnMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Djinn"/> that will be taken.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public TakeDjinnMove(GameState state0, int index, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.index = index;
            this.after = after;
        }

        /// <summary>
        /// Gets the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public Djinn Djinn
        {
            get { return this.State.VisibleDjinns[this.index]; }
        }

        /// <summary>
        /// Gets the index of the <see cref="Djinn"/> that will be taken.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Take {0}", this.Djinn);
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                inventory: state0.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(state0.VisibleDjinns[this.index]))),
                visibleDjinns: state0.VisibleDjinns.RemoveAt(this.index)));
        }
    }
}
