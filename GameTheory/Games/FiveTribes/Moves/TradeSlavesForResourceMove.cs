// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to trade a pair of <see cref="FiveTribes.Resource.Slave">Slaves</see> for the specified <see cref="Resource"/>.
    /// </summary>
    public class TradeSlavesForResourceMove : Move
    {
        private readonly Resource resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeSlavesForResourceMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="owner">The player who may perform this move.</param>
        /// <param name="resource">The <see cref="Resource"/> that will received by the player.</param>
        public TradeSlavesForResourceMove(GameState state, PlayerToken owner, Resource resource)
            : base(state, owner)
        {
            this.resource = resource;
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> that will be received by the player.
        /// </summary>
        public Resource Resource
        {
            get { return this.resource; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Trade {Resource.Slave},{Resource.Slave} for {this.resource}";
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory[this.Player];
            return state.With(
                inventory: state.Inventory.SetItem(this.Player, inventory.With(resources: inventory.Resources.Remove(Resource.Slave, 2).Add(this.resource))));
        }
    }
}
