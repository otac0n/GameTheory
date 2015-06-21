// -----------------------------------------------------------------------
// <copyright file="ScoreBuildersInHandMove.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Linq;

    public class ScoreBuildersInHandMove : Move
    {
        private readonly int boost;

        public ScoreBuildersInHandMove(GameState state0, int boost)
            : base(state0, state0.ActivePlayer)
        {
            this.boost = boost;
        }

        public override string ToString()
        {
            return string.Format("Score {0}", string.Join(",", this.State.InHand));
        }

        internal override GameState Apply(GameState state0)
        {
            var blueTiles = Sultanate.GetSquarePoints(state0.LastPoint).Count(p => state0.Sultanate[p].Tile.Color == TileColor.Blue);
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];
            var score = (state0.InHand.Count + this.boost) * blueTiles * state0.ScoreTables[player].BuilderMultiplier;

            return state0.With(
                bag: state0.Bag.AddRange(state0.InHand),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state0.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins + score)),
                phase: Phase.TileAction);
        }
    }
}
