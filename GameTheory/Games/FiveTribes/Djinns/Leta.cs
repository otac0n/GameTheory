// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave"/> to take control of 1 empty Tile (no Camel, Meeple, Palm Tree or Palace); place 1 of your Camels on it.
    /// </summary>
    public class Leta : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Leta"/>.
        /// </summary>
        public static readonly Leta Instance = new Leta();

        private Leta()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override bool CanGetMoves(GameState state)
        {
            return base.CanGetMoves(state) && state.IsPlayerUnderCamelLimit(state.ActivePlayer);
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state)
        {
            var emptySquares = Enumerable.Range(0, Sultanate.Width * Sultanate.Height).Where(i =>
            {
                var sq = state.Sultanate[i];
                return sq.Owner == null && sq.Meeples.Count == 0 && sq.Palaces == 0 && sq.PalmTrees == 0;
            });

            return from i in emptySquares
                   select new PlaceCamelMove(state, i);
        }
    }
}
