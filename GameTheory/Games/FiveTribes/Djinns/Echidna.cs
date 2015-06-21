// -----------------------------------------------------------------------
// <copyright file="Echidna.cs" company="(none)">
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
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave" /> to activate the <see cref="DoubleBuilderScoreMove" />.
    /// </summary>
    public class Echidna : Djinn.PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Echidna" />.
        /// </summary>
        public static readonly Echidna Instance = new Echidna();

        private Echidna()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override GameState CleanUp(GameState state)
        {
            var player = state.Inventory.Where(i => i.Value.Djinns.Contains(this)).Select(i => i.Key).Single();
            var scoreTable = state.ScoreTables[player];

            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, scoreTable.With(builderMultiplier: scoreTable.BuilderMultiplier / 2)));
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            yield return new DoubleBuilderScoreMove(state0);
        }

        /// <summary>
        /// Double the amount of DGs your Builders get this turn.
        /// </summary>
        public class DoubleBuilderScoreMove : Move
        {
            public DoubleBuilderScoreMove(GameState state0)
                : base(state0, state0.ActivePlayer)
            {
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return "Double the amout of GCs your Builders get this turn";
            }

            internal override GameState Apply(GameState state0)
            {
                var player = state0.ActivePlayer;
                var scoreTable = state0.ScoreTables[player];

                return state0.With(
                    scoreTables: state0.ScoreTables.SetItem(player, scoreTable.With(builderMultiplier: scoreTable.BuilderMultiplier * 2)));
            }
        }
    }
}
