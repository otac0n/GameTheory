// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave"/> to double the amount of GCs your Builders get this turn.
    /// </summary>
    public sealed class Echidna : PayPerActionDjinnBase
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
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var player = state.Inventory.Where(i => i.Value.Djinns.Contains(this)).Select(i => i.Key).Single();
            var scoreTable = state.ScoreTables[player];

            return state.With(
                scoreTables: state.ScoreTables.SetItem(player, scoreTable.With(builderMultiplier: scoreTable.BuilderMultiplier / 2)));
        }

        /// <inheritdoc />
        protected override InterstitialState GetInterstitialState() => new Paid();

        private class Paid : InterstitialState
        {
            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                yield return new DoubleBuilderScoreMove(state);
            }
        }
    }
}
