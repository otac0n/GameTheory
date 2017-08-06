// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;

    /// <summary>
    /// Represents a move for the payment of a specific amount of <see cref="Meeple">Meeples</see> and <see cref="Resource">Resources</see>.
    /// </summary>
    public class PayMeeplesAndResourcesMove : Move
    {
        private readonly Func<GameState, GameState> after;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayMeeplesAndResourcesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeple">The <see cref="Meeple"/> portion of the cost.</param>
        /// <param name="resource">The <see cref="Resource"/> portion of the cost.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesAndResourcesMove(GameState state, Meeple meeple, Resource resource, Func<GameState, GameState> after)
            : this(state, new EnumCollection<Meeple>(meeple), new EnumCollection<Resource>(resource), after)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayMeeplesAndResourcesMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="meeples">The <see cref="Meeple"/> portion of the cost.</param>
        /// <param name="resources">The <see cref="Resource"/> portion of the cost.</param>
        /// <param name="after">A function to perform after the move has taken place.</param>
        public PayMeeplesAndResourcesMove(GameState state, EnumCollection<Meeple> meeples, EnumCollection<Resource> resources, Func<GameState, GameState> after)
            : base(state)
        {
            this.after = after;
            this.Meeples = meeples;
            this.Resources = resources;
        }

        /// <summary>
        /// Gets the <see cref="Meeple"/> portion of the cost.
        /// </summary>
        public EnumCollection<Meeple> Meeples { get; }

        /// <summary>
        /// Gets the <see cref="Resource"/> portion of the cost.
        /// </summary>
        public EnumCollection<Resource> Resources { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <inheritdoc />
        public override string ToString() => $"Pay {this.Meeples}{(this.Meeples.Count > 0 && this.Resources.Count > 0 ? ", " : string.Empty)}{this.Resources}";

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            return this.after(state.With(
                bag: state.Bag.AddRange(this.Meeples),
                inventory: state.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.RemoveRange(this.Meeples), resources: inventory.Resources.RemoveRange(this.Resources))),
                resourceDiscards: state.ResourceDiscards.AddRange(this.Resources)));
        }
    }
}
