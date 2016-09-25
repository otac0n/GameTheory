// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> to choose an empty Tile (with no Camel, Meeple, Palm Tree or Palace). Place 3 Meeples on that tile (drawn at random from the bag).
    /// </summary>
    public class AnunNak : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="AnunNak"/>.
        /// </summary>
        public static readonly AnunNak Instance = new AnunNak();

        private AnunNak()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        public override string Name
        {
            get { return "Anun-Nak"; }
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state)
        {
            Contract.Requires(state != null);

            var toDraw = Math.Min(state.Bag.Count, 3);
            if (toDraw == 0)
            {
                return Enumerable.Empty<Move>();
            }

            return from i in Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                   let sq = state.Sultanate[i]
                   where sq.Owner == null && sq.Meeples.Count == 0 && sq.Palaces == 0 && sq.PalmTrees == 0
                   select new AddMeeplesMove(state, i);
        }
    }
}
