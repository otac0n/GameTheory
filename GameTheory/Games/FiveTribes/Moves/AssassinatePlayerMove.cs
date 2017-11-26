// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to assassinate specific <see cref="Meeple">Meeples</see> from a specific player's inventory.
    /// </summary>
    public sealed class AssassinatePlayerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinatePlayerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="victim">The <see cref="PlayerToken"/> whose <see cref="Meeple">Meeples</see> will be assassinated.</param>
        /// <param name="meeples">The <see cref="Meeple">Meeples</see> that will be assassinated.</param>
        public AssassinatePlayerMove(GameState state, PlayerToken victim, EnumCollection<Meeple> meeples)
            : base(state)
        {
            this.Meeples = meeples;
            this.Victim = victim;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Assassinate ", this.Meeples, " in front of ", this.Victim };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the <see cref="Meeple">Meeples</see> that will be assassinated.
        /// </summary>
        public EnumCollection<Meeple> Meeples { get; }

        /// <summary>
        /// Gets the <see cref="PlayerToken"/> whose <see cref="Meeple">Meeples</see> will be assassinated.
        /// </summary>
        public PlayerToken Victim { get; }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory[this.Victim];
            var newState = state.With(
                bag: state.Bag.AddRange(state.InHand).AddRange(this.Meeples),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state.Inventory.SetItem(this.Victim, inventory.With(meeples: inventory.Meeples.RemoveRange(this.Meeples))));

            foreach (var owner in newState.Players)
            {
                foreach (var djinn in newState.Inventory[owner].Djinns)
                {
                    newState = djinn.HandleAssassination(owner, newState, this.Victim, this.Meeples);
                }
            }

            return newState.With(phase: Phase.TileAction);
        }
    }
}
