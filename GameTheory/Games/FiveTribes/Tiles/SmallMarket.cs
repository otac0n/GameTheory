// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System;
    using System.Collections.Generic;
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
            if (state.VisibleResources.Count > 0)
            {
                var moves = Cost.Gold(state, Gold, s => s.WithInterstitialState(new ChoosingResource()));

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

        private class ChoosingResource : InterstitialState
        {
            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                var available = Math.Min(FirstN, state.VisibleResources.Count);
                for (var i = 0; i < available; i++)
                {
                    yield return new TakeResourceMove(state, i, s1 => s1.With(phase: Phase.MerchandiseSale));
                }
            }

            public override int CompareTo(InterstitialState other)
            {
                if (other is ChoosingResource)
                {
                    return 0;
                }
                else
                {
                    return base.CompareTo(other);
                }
            }
        }
    }
}
