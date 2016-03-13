// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to sell merchandise in exchange for Gold Coins (GC).
    /// </summary>
    public class SellMerchandiseMove : Move
    {
        private readonly EnumCollection<Resource> resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellMerchandiseMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="resources">The <see cref="Resource">Resources</see> being sold.</param>
        public SellMerchandiseMove(GameState state0, EnumCollection<Resource> resources)
            : base(state0, state0.ActivePlayer)
        {
            this.resources = resources;
        }

        /// <summary>
        /// Gets the <see cref="Resource">Resources</see> being sold.
        /// </summary>
        public EnumCollection<Resource> Resources
        {
            get { return this.resources; }
        }

        /// <summary>
        /// Gets the value of the <see cref="Resource">Resources</see> being sold, in Gold Coins (GC).
        /// </summary>
        public int Value
        {
            get { return GameState.ScoreResources(this.resources); }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Trade {0} for {1}", string.Join(",", this.resources), GameState.SuitValues[this.resources.Count]);
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return state0.With(
                inventory: state0.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.RemoveRange(this.resources), goldCoins: inventory.GoldCoins + this.Value)),
                resourceDiscards: state0.ResourceDiscards.AddRange(this.resources));
        }
    }
}
