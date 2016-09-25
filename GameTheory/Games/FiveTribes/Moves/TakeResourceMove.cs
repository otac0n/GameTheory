// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move to take a specified <see cref="Resource"/>.
    /// </summary>
    public class TakeResourceMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeResourceMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Resource"/> that will be taken.</param>
        public TakeResourceMove(GameState state, int index)
            : this(state, index, s => s)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeResourceMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the <see cref="Resource"/> that will be taken.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public TakeResourceMove(GameState state, int index, Func<GameState, GameState> after)
            : base(state, state.ActivePlayer)
        {
            this.index = index;
            this.after = after;
        }

        /// <summary>
        /// Gets the index of the <see cref="Resource"/> that will be taken.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> that will be taken.
        /// </summary>
        public Resource Resource
        {
            get { return this.State.VisibleResources[this.index]; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Take {this.State.VisibleResources[this.index]}";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                inventory: state.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.Add(state.VisibleResources[this.index]))),
                visibleResources: state.VisibleResources.RemoveAt(this.index)));
        }
    }
}
