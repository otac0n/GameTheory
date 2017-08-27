// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to sell merchandise in exchange for Gold Coins (GC).
    /// </summary>
    public sealed class SellMerchandiseMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SellMerchandiseMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="resources">The <see cref="Resource">Resources</see> being sold.</param>
        public SellMerchandiseMove(GameState state, EnumCollection<Resource> resources)
            : base(state)
        {
            this.Resources = resources;
        }

        /// <summary>
        /// Gets the <see cref="Resource">Resources</see> being sold.
        /// </summary>
        public EnumCollection<Resource> Resources { get; }

        /// <summary>
        /// Gets the value of the <see cref="Resource">Resources</see> being sold, in Gold Coins (GC).
        /// </summary>
        public int Value => GameState.ScoreResources(this.Resources);

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Trade ", this.Resources, " for ", GameState.SuitValues[this.Resources.Count] };

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return state.With(
                inventory: state.Inventory.SetItem(player, inventory.With(resources: inventory.Resources.RemoveRange(this.Resources), goldCoins: inventory.GoldCoins + this.Value)),
                resourceDiscards: state.ResourceDiscards.AddRange(this.Resources));
        }
    }
}
