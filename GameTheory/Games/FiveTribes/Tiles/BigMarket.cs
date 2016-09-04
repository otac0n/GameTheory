// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Represents a Big Market tile.
    /// </summary>
    public class BigMarket : Tile
    {
        /// <summary>
        /// The number of <see cref="Resource">Resources</see> the player can choose from, starting from the beginning.
        /// </summary>
        public const int FirstN = 6;

        /// <summary>
        /// The cost of using the <see cref="BigMarket"/> to purchase two <see cref="Resource">Resources</see>.
        /// </summary>
        public const int Gold = 6;

        /// <summary>
        /// The singleton instance of <see cref="BigMarket"/>.
        /// </summary>
        public static readonly BigMarket Instance = new BigMarket();

        private BigMarket()
            : base(4, TileColor.Red)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            if (state.VisibleResources.Count > 0)
            {
                var moves = Cost.Gold(state, Gold, s => s, s1 => from i in Enumerable.Range(0, Math.Min(FirstN, s1.VisibleResources.Count))
                                                                 select new TakeResourceMove(s1, i, s2 =>
                                                                 {
                                                                     if (s2.VisibleResources.Count >= 1)
                                                                     {
                                                                         return s2.WithMoves(s3 => Enumerable.Concat(
                                                                             from j in Enumerable.Range(0, Math.Min(FirstN - 1, s3.VisibleResources.Count))
                                                                             select new TakeResourceMove(s3, j, s4 => s4.With(phase: Phase.CleanUp)),
                                                                             new Move[] { new ChangePhaseMove(s3, "Skip second resource", Phase.CleanUp) }));
                                                                     }
                                                                     else
                                                                     {
                                                                         return s2.With(phase: Phase.CleanUp);
                                                                     }
                                                                 }));

                foreach (var m in moves)
                {
                    yield return m;
                }
            }

            foreach (var m in base.GetTileActionMoves(state))
            {
                yield return m;
            }
        }
    }
}
