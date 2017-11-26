// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Tiles
{
    using System.Collections.Generic;
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
                var moves = Cost.OneElderPlusOneElderOrOneSlave(state, s => s.WithInterstitialState(new ChoosingDjinn()));

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

        private class ChoosingDjinn : InterstitialState
        {
            public override int CompareTo(InterstitialState other)
            {
                if (other is ChoosingDjinn)
                {
                    return 0;
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                for (var i = 0; i < state.VisibleDjinns.Count; i++)
                {
                    yield return new TakeDjinnMove(state, i, s1 => s1.With(phase: Phase.MerchandiseSale));
                }
            }
        }
    }
}
