﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> to let your Assassins kill 2 Meeples of any color on the same Tile or kill 2 Elders and/or Viziers from the same opponent.
    /// </summary>
    public sealed class Ibus : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Ibus"/>.
        /// </summary>
        public static readonly Ibus Instance = new Ibus();

        private Ibus()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        public override string Name => Resources.Ibus;

        /// <inheritdoc />
        protected override InterstitialState InterstitialState => new Paid();

        /// <inheritdoc />
        protected override GameState CleanUp(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            var player = state.Inventory.Where(i => i.Value.Djinns.Contains(this)).Select(i => i.Key).Single();
            var assassinationTable = state.AssassinationTables[player];

            return state.With(
                assassinationTables: state.AssassinationTables.SetItem(player, assassinationTable.With(killCount: 1)));
        }

        private class Paid : InterstitialState
        {
            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                yield return new DoubleAssassinKillCountMove(state);
            }
        }
    }
}
