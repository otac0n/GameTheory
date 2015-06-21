// -----------------------------------------------------------------------
// <copyright file="Sibittis.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Pay <see cref="Cost.OneElderPlusOneElderOrOneSlave" /> to activate the <see cref="DrawDjinnsMove" />.
    /// </summary>
    public class Sibittis : Djinn.PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="Sibittis" />.
        /// </summary>
        public static readonly Sibittis Instance = new Sibittis();

        private Sibittis()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        protected override bool CanGetMoves(GameState state)
        {
            return base.CanGetMoves(state) && (state.DjinnPile.Count + state.DjinnDiscards.Count) >= 1;
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            yield return new DrawDjinnsMove(state0);
        }

        /// <summary>
        /// Draw the top 3 Djinns from the top of the Djinns pile; keep 1, discard the 2 others.
        /// </summary>
        public class DrawDjinnsMove : Move
        {
            public DrawDjinnsMove(GameState state0)
                : base(state0, state0.ActivePlayer)
            {
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return string.Format("Draw {0} Djinns", GetDrawCount(this.State));
            }

            internal override GameState Apply(GameState state0)
            {
                var toDraw = GetDrawCount(state0);

                ImmutableList<Djinn> dealt;
                var newDjinnDiscards = state0.DjinnDiscards;
                var newDjinnPile = state0.DjinnPile.Deal(toDraw, out dealt, ref newDjinnDiscards);

                var s1 = state0.With(
                    djinnPile: newDjinnPile,
                    djinnDiscards: newDjinnDiscards);

                return s1.WithMoves(s2 => Enumerable.Range(0, toDraw).Select(i => new TakeDealtDjinnMove(s2, dealt, i)));
            }

            private static int GetDrawCount(GameState state0)
            {
                return Math.Min(3, state0.DjinnPile.Count + state0.DjinnDiscards.Count);
            }
        }

        public class TakeDealtDjinnMove : Move
        {
            private readonly ImmutableList<Djinn> dealt;
            private readonly int index;

            public TakeDealtDjinnMove(GameState state0, ImmutableList<Djinn> dealt, int index)
                : base(state0, state0.ActivePlayer)
            {
                this.dealt = dealt;
                this.index = index;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return string.Format("Take {0}", this.dealt[this.index]);
            }

            internal override GameState Apply(GameState state0)
            {
                var player = state0.ActivePlayer;
                var inventory = state0.Inventory[player];

                return state0.With(
                    djinnDiscards: state0.DjinnDiscards.AddRange(this.dealt.RemoveAt(this.index)),
                    inventory: state0.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(this.dealt[this.index]))));
            }
        }
    }
}
