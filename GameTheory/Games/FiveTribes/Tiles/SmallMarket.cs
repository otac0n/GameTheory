// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Represents a Small Market tile.
    /// </summary>
    public class SmallMarket : Tile
    {
        /// <summary>
        /// The number of <see cref="Resource">Resources</see> the player can choose from, starting from the beginning.
        /// </summary>
        public const int FirstN = 3;

        /// <summary>
        /// The cost of using the <see cref="SmallMarket"/> to purchase a <see cref="Resource"/>.
        /// </summary>
        public const int Gold = 3;

        /// <summary>
        /// The singleton instance of <see cref="SmallMarket"/>.
        /// </summary>
        public static readonly SmallMarket Instance = new SmallMarket();

        private SmallMarket()
            : base(6, TileColor.Red)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            if (state.VisibleResources.Count >= 1)
            {
                var moves = Cost.Gold(state, Gold, s => s, s1 => from i in Enumerable.Range(0, Math.Min(FirstN, s1.VisibleResources.Count))
                                                                 select new TakeResourceMove(s1, i, s2 => s2.With(phase: Phase.CleanUp)));

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
