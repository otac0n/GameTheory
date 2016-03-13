// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave"/> to double the amount of GCs your Builders get this turn.
    /// </summary>
    public class Echidna : Djinn.PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Echidna"/>.
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
        /// Represents a move to double the amount of Gold Coins (GCs) the active player's <see cref="Meeple.Builder">Builders</see> get this turn.
        /// </summary>
        public class DoubleBuilderScoreMove : Move
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DoubleBuilderScoreMove"/> class.
            /// </summary>
            /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
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
