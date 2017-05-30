// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to trade a pair of <see cref="FiveTribes.Resource.Slave">Slaves</see> for the specified <see cref="Resource"/>.
    /// </summary>
    public class TradeSlavesForResourceMove : Move
    {
        private static readonly EnumCollection<Resource> Cost = new EnumCollection<Resource>(Resource.Slave, Resource.Slave);

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeSlavesForResourceMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="owner">The player who may perform this move.</param>
        /// <param name="resource">The <see cref="Resource"/> that will received by the player.</param>
        public TradeSlavesForResourceMove(GameState state, PlayerToken owner, Resource resource)
            : base(state, owner)
        {
            this.Resource = resource;
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> that will be received by the player.
        /// </summary>
        public Resource Resource { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Trade {Cost} for {this.Resource}";

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory[this.PlayerToken];
            return state.With(
                inventory: state.Inventory.SetItem(this.PlayerToken, inventory.With(resources: inventory.Resources.RemoveRange(Cost).Add(this.Resource))));
        }
    }
}
