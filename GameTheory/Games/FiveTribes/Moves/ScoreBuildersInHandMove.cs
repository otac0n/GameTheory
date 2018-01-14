// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to score the <see cref="Meeple.Builder">Builders</see> in hand.
    /// </summary>
    public sealed class ScoreBuildersInHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreBuildersInHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="spentSlaves">The number of <see cref="Resource.Slave">Slaves</see> spent.</param>
        public ScoreBuildersInHandMove(GameState state, int spentSlaves)
            : base(state)
        {
            this.SpentSlaves = spentSlaves;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Score ", this.State.InHand };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        /// <summary>
        /// Gets the number of <see cref="Resource.Slave">Slaves</see> spent.
        /// </summary>
        public int SpentSlaves { get; }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            yield return new ScoreBuildersInHandMove(state, 0);

            var moves = Cost.OneOrMoreSlaves(state, (s, paid) => s.WithInterstitialState(new ScoringWithBonus(paid)));
            foreach (var move in moves)
            {
                yield return move;
            }
        }

        internal override GameState Apply(GameState state)
        {
            var blueTiles = Sultanate.GetSquarePoints(state.LastPoint).Count(p => state.Sultanate[p].Tile.Color == TileColor.Blue);
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];
            var score = (state.InHand.Count + this.SpentSlaves) * blueTiles * state.ScoreTables[player].BuilderMultiplier;

            return state.With(
                bag: state.Bag.AddRange(state.InHand),
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state.Inventory.SetItem(player, inventory.With(goldCoins: inventory.GoldCoins + score)),
                phase: Phase.TileAction);
        }

        private class ScoringWithBonus : InterstitialState
        {
            private int paid;

            public ScoringWithBonus(int paid)
            {
                this.paid = paid;
            }

            public override int CompareTo(InterstitialState other)
            {
                if (other is ScoringWithBonus s)
                {
                    return this.paid.CompareTo(s.paid);
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                yield return new ScoreBuildersInHandMove(state, this.paid);
            }
        }
    }
}
