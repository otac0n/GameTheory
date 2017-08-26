// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System.Collections.Generic;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Pay <see cref="Cost.OneSlave"/> to take the top card from the Resource pile.
    /// </summary>
    public class Sloar : PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Sloar"/>.
        /// </summary>
        public static readonly Sloar Instance = new Sloar();

        private Sloar()
            : base(8, Cost.OneSlave)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state)
        {
            yield return new DrawTopCardMove(state);
        }
    }
}
