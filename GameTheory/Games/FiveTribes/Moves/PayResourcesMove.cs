// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move for the payment of a specific amount of <see cref="Resource">Resources</see>.
    /// </summary>
    public class PayResourcesMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Resource> resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayResourcesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="resource">The cost, in <see cref="Resource">Resources</see>.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayResourcesMove(GameState state, Resource resource, Func<GameState, GameState> after)
            : this(state, new EnumCollection<Resource>(resource), after)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayResourcesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="resources">The cost, in <see cref="Resource">Resources</see>.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayResourcesMove(GameState state, EnumCollection<Resource> resources, Func<GameState, GameState> after)
            : base(state, state.ActivePlayer)
        {
            this.after = after;
            this.resources = resources;
        }

        /// <summary>
        /// Gets the cost, in <see cref="Resource">Resources</see>.
        /// </summary>
        public EnumCollection<Resource> Resources
        {
            get { return this.resources; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Pay {string.Join(",", this.resources)}";
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                inventory: state.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.RemoveRange(this.resources))),
                resourceDiscards: state.ResourceDiscards.AddRange(this.resources)));
        }
    }
}
