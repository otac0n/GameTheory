// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    /// <summary>
    /// Represents a move to take all of the <see cref="Meeple">Meeples</see> in hand, and put them in the active player's inventory.
    /// </summary>
    public class TakeMeeplesInHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeMeeplesInHandMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        public TakeMeeplesInHandMove(GameState state0)
            : base(state0, state0.ActivePlayer)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Take {0}", string.Join(",", this.State.InHand));
        }

        internal override GameState Apply(GameState state0)
        {
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];
            return state0.With(
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state0.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.AddRange(state0.InHand))),
                phase: Phase.TileAction);
        }
    }
}
