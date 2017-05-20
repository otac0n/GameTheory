// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Djinns;

    /// <summary>
    /// Represent a move to draw the top three <see cref="Djinn">Djinns</see> from the Djinn pile.
    /// </summary>
    internal class DrawDjinnsMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawDjinnsMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DrawDjinnsMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Draw {GetDrawCount(this.State)} Djinns";
        }

        internal override GameState Apply(GameState state)
        {
            var toDraw = GetDrawCount(state);

            var newDjinnDiscards = state.DjinnDiscards;
            var newDjinnPile = state.DjinnPile.Deal(toDraw, out ImmutableList<Djinn> dealt, ref newDjinnDiscards);

            var s1 = state.With(
                djinnPile: newDjinnPile,
                djinnDiscards: newDjinnDiscards);

            return s1.WithMoves(s2 => Enumerable.Range(0, toDraw).Select(i => new TakeDealtDjinnMove(s2, dealt, i)));
        }

        private static int GetDrawCount(GameState state)
        {
            return Math.Min(3, state.DjinnPile.Count + state.DjinnDiscards.Count);
        }
    }
}
