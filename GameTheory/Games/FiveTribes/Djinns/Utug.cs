// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave"/> to take control of 1 Tile with only Meeples on it (no Camel, Palm Tree or Palace); place 1 of your Camels on it.
    /// </summary>
    public sealed class Utug : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Utug"/>.
        /// </summary>
        public static readonly Utug Instance = new Utug();

        private Utug()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override InterstitialState InterstitialState => new ChoosingSquare();

        /// <inheritdoc />
        protected override bool CanGetMoves(GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return base.CanGetMoves(state) && state.IsPlayerUnderCamelLimit(state.ActivePlayer);
        }

        private class ChoosingSquare : InterstitialState
        {
            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                return from i in Enumerable.Range(0, Sultanate.Size.Count)
                       let sq = state.Sultanate[i]
                       where sq.Owner == null && sq.Meeples.Count >= 1 && sq.Palaces == 0 && sq.PalmTrees == 0
                       select new PlaceCamelMove(state, Sultanate.Size[i]);
            }
        }
    }
}
