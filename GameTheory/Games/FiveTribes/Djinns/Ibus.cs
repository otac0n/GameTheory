// -----------------------------------------------------------------------
// <copyright file="Ibus.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave" /> to activate the <see cref="DoubleAssassinKillCountMove" />.
    /// </summary>
    public class Ibus : Djinn.PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Ibus" />.
        /// </summary>
        public static readonly Ibus Instance = new Ibus();

        private Ibus()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override GameState CleanUp(GameState state)
        {
            var player = state.Inventory.Where(i => i.Value.Djinns.Contains(this)).Select(i => i.Key).Single();
            var assassinationTable = state.AssassinationTables[player];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(player, assassinationTable.With(killCount: 1)));
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            yield return new DoubleAssassinKillCountMove(state0);
        }

        /// <summary>
        /// Your Assassins kill 2 Meeples of any color on the same Tile or kill 2 Elders and/or Viziers from the same opponent.
        /// </summary>
        public class DoubleAssassinKillCountMove : Move
        {
            public DoubleAssassinKillCountMove(GameState state0)
                : base(state0, state0.ActivePlayer)
            {
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return "Double the number of meeples your Assassins kill this turn";
            }

            internal override GameState Apply(GameState state0)
            {
                var player = state0.ActivePlayer;
                var assassinationTable = state0.AssassinationTables[player];

                return state0.With(
                    assassinationTables: state0.AssassinationTables.SetItem(player, assassinationTable.With(killCount: 2)));
            }
        }
    }
}
