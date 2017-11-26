// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System;
    using System.Collections.Generic;
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
        /// The number of <see cref="Resource">Resources</see> the player may choose.
        /// </summary>
        public const int Resources = 2;

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
                var moves = Cost.Gold(state, Gold, s => s.WithInterstitialState(new ChoosingResource(Resources)));

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
            private readonly int remaining;

            public ChoosingResource(int remaining)
            {
                this.remaining = remaining;
            }

            public override int CompareTo(InterstitialState other)
            {
                if (other is ChoosingResource c)
                {
                    return this.remaining.CompareTo(c.remaining);
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                var available = Math.Min(FirstN - (Resources - this.remaining), state.VisibleResources.Count);
                for (var i = 0; i < available; i++)
                {
                    yield return new TakeResourceMove(state, i, s1 =>
                    {
                        var remaining = this.remaining - 1;
                        if (s1.VisibleResources.Count > 0 && remaining > 0)
                        {
                            return s1.WithInterstitialState(new ChoosingResource(remaining));
                        }
                        else
                        {
                            return s1.With(phase: Phase.MerchandiseSale);
                        }
                    });
                }

                if (this.remaining < Resources)
                {
                    yield return new ChangePhaseMove(state, "Skip remaining resources", Phase.MerchandiseSale);
                }
            }
        }
    }
}
