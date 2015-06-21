// -----------------------------------------------------------------------
// <copyright file="PayMeeplesAndResourcesMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents a move for the payment of a specific amount of <see cref="Meeple">Meeples</see> and <see cref="Resource">Resources</see>.
    /// </summary>
    public class PayMeeplesAndResourcesMove : Move
    {
        private readonly Func<GameState, GameState> after;
        private readonly EnumCollection<Meeple> meeples;
        private readonly EnumCollection<Resource> resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayMeeplesAndResourcesMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeple">The <see cref="Meeple"/> portion of the cost.</param>
        /// <param name="resource">The <see cref="Resource"/> portion of the cost.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesAndResourcesMove(GameState state0, Meeple meeple, Resource resource, Func<GameState, GameState> after)
            : this(state0, new EnumCollection<Meeple>(meeple), new EnumCollection<Resource>(resource), after)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayMeeplesAndResourcesMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeples">The <see cref="Meeple"/> portion of the cost.</param>
        /// <param name="resources">The <see cref="Resource"/> portion of the cost.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesAndResourcesMove(GameState state0, EnumCollection<Meeple> meeples, EnumCollection<Resource> resources, Func<GameState, GameState> after)
            : base(state0, state0.ActivePlayer)
        {
            this.after = after;
            this.meeples = meeples;
            this.resources = resources;
        }

        /// <summary>
        /// Gets the <see cref="Meeple"/> portion of the cost.
        /// </summary>
        public EnumCollection<Meeple> Meeples
        {
            get { return this.meeples; }
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> portion of the cost.
        /// </summary>
        public EnumCollection<Resource> Resources
        {
            get { return this.resources; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Pay {0}", string.Join(",", this.meeples.Cast<object>().Concat(this.resources.Cast<object>())));
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];

            return this.after(state0.With(
                bag: state0.Bag.AddRange(this.meeples),
                inventory: state0.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(this.meeples), resources: inventory.Resources.RemoveRange(this.resources))),
                resourceDiscards: state0.ResourceDiscards.AddRange(this.resources)));
        }
    }
}
