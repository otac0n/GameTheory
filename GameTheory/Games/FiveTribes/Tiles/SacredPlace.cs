// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Represents a Sacred Place tile.
    /// </summary>
    public class SacredPlace : Tile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SacredPlace"/> class.
        /// </summary>
        /// <param name="value">The value of the <see cref="Tile"/>, in Victory Points (VP).</param>
        public SacredPlace(int value)
            : base(value, TileColor.Blue)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            if (state.VisibleDjinns.Count >= 1)
            {
                var moves = Cost.OneElderPlusOneElderOrOneSlave(state, s => s, s1 => from i in Enumerable.Range(0, s1.VisibleDjinns.Count)
                                                                                     select new TakeDjinnMove(s1, i, s2 => s2.With(phase: Phase.MerchandiseSale)));

                foreach (var move in moves)
                {
                    yield return move;
                }
            }

            foreach (var baseMove in base.GetTileActionMoves(state))
            {
                yield return baseMove;
            }
        }
    }
}
