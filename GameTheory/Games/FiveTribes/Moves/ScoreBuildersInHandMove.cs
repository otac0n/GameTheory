// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Linq;

    /// <summary>
    /// Represents a move to score the <see cref="Meeple.Builder">Builders</see> in hand.
    /// </summary>
    public class ScoreBuildersInHandMove : Move
    {
        private readonly int spentSlaves;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreBuildersInHandMove"/> class.
        /// </summary>
        /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="spentSlaves">The number of <see cref="Resource.Slave">Slaves</see> spent.</param>
        public ScoreBuildersInHandMove(GameState state0, int spentSlaves)
            : base(state0, state0.ActivePlayer)
        {
            this.spentSlaves = spentSlaves;
        }

        /// <summary>
        /// Gets the number of <see cref="Resource.Slave">Slaves</see> spent.
        /// </summary>
        public int SpentSlaves
        {
            get { return this.spentSlaves; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Score {0}", string.Join(",", this.State.InHand));
        }

        internal override GameState Apply(GameState state0)
        {
            var blueTiles = Sultanate.GetSquarePoints(state0.LastPoint).Count(p => state0.Sultanate[p].Tile.Color == TileColor.Blue);
            var player = state0.ActivePlayer;
            var inventory = state0.Inventory[player];
            var score = (state0.InHand.Count + this.spentSlaves) * blueTiles * state0.ScoreTables[player].BuilderMultiplier;

            return state0.With(
                bag: state0.Bag.AddRange(state0.InHand),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state0.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins + score)),
                phase: Phase.TileAction);
        }
    }
}
