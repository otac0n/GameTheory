// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave"/> to draw the top 3 Djinns from the top of the Djinns pile; keep 1, discard the 2 others.
    /// </summary>
    public class Sibittis : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Sibittis"/>.
        /// </summary>
        public static readonly Sibittis Instance = new Sibittis();

        private Sibittis()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override bool CanGetMoves(GameState state)
        {
            return base.CanGetMoves(state) && (state.DjinnPile.Count + state.DjinnDiscards.Count) >= 1;
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state)
        {
            yield return new DrawDjinnsMove(state);
        }
    }
}
