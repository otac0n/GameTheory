// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

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
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="spentSlaves">The number of <see cref="Resource.Slave">Slaves</see> spent.</param>
        public ScoreBuildersInHandMove(GameState state, int spentSlaves)
            : base(state)
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
        public override string ToString() => $"Score {this.State.InHand}";

        internal override GameState Apply(GameState state)
        {
            var blueTiles = Sultanate.GetSquarePoints(state.LastPoint).Count(p => state.Sultanate[p].Tile.Color == TileColor.Blue);
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];
            var score = (state.InHand.Count + this.spentSlaves) * blueTiles * state.ScoreTables[player].BuilderMultiplier;

            return state.With(
                bag: state.Bag.AddRange(state.InHand),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins + score)),
                phase: Phase.TileAction);
        }
    }
}
