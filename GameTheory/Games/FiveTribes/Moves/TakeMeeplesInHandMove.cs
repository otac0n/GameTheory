// -----------------------------------------------------------------------
// <copyright file="TakeMeeplesInHandMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    public class TakeMeeplesInHandMove : Move
    {
        public TakeMeeplesInHandMove(GameState state0)
            : base(state0, state0.ActivePlayer)
        {
        }

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
