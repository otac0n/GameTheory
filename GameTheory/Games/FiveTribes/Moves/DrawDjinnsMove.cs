// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Represent a move to draw the top three <see cref="Djinn">Djinns</see> from the Djinn pile.
    /// </summary>
    public sealed class DrawDjinnsMove : Move
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
        public override IList<object> FormatTokens => new object[] { "Draw ", GetDrawCount(this.State), " Djinns" };

        /// <inheritdoc />
        public override bool IsDeterministic => false;

        internal override GameState Apply(GameState state)
        {
            var toDraw = GetDrawCount(state);

            var newDjinnDiscards = state.DjinnDiscards;
            var newDjinnPile = state.DjinnPile.Deal(toDraw, out ImmutableList<Djinn> dealt, ref newDjinnDiscards);

            var s1 = state.With(
                djinnPile: newDjinnPile,
                djinnDiscards: newDjinnDiscards);

            return s1.WithInterstitialState(new ChoosingDjinn(dealt));
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            // TODO: Implement outcomes.
            return base.GetOutcomes(state);
        }

        private static int GetDrawCount(GameState state) => Math.Min(3, state.DjinnPile.Count + state.DjinnDiscards.Count);

        private class ChoosingDjinn : InterstitialState
        {
            private ImmutableList<Djinn> dealt;

            public ChoosingDjinn(ImmutableList<Djinn> dealt)
            {
                this.dealt = dealt;
            }

            public override int CompareTo(InterstitialState other)
            {
                if (other is ChoosingDjinn s)
                {
                    return CompareUtilities.CompareLists(this.dealt, s.dealt);
                }
                else
                {
                    return base.CompareTo(other);
                }
            }

            public override IEnumerable<Move> GenerateMoves(GameState state)
            {
                for (var i = 0; i < this.dealt.Count; i++)
                {
                    yield return new TakeDealtDjinnMove(state, this.dealt, i);
                }
            }
        }
    }
}
