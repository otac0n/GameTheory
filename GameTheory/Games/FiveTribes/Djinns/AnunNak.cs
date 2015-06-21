// -----------------------------------------------------------------------
// <copyright file="AnunNak.cs" company="(none)">
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
    /// Pay <see cref="Cost.OneElderOrOneSlave"/> to choose an empty Tile (with no Camel, Meeple, Palm Tree or Palace). Place 3 Meeples on that tile (drawn at random from the bag).
    /// </summary>
    public class AnunNak : Djinn.PayPerActionDjinnBase
    {
        /// <summary>
        /// The singleton instance of <see cref="AnunNak"/>.
        /// </summary>
        public static readonly AnunNak Instance = new AnunNak();

        private AnunNak()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        /// <inheritdoc />
        public override string Name
        {
            get { return "Anun-Nak"; }
        }

        /// <inheritdoc />
        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            var toDraw = Math.Min(state0.Bag.Count, 3);
            if (toDraw == 0)
            {
                return Enumerable.Empty<Move>();
            }

            return from i in Enumerable.Range(0, Sultanate.Width * Sultanate.Height)
                   let sq = state0.Sultanate[i]
                   where sq.Owner == null && sq.Meeples.Count == 0 && sq.Palaces == 0 && sq.PalmTrees == 0
                   select new AddMeeplesMove(state0, i);
        }

        /// <summary>
        /// Represents a move to draw three <see cref="Meeple">Meeples</see> and place them on the specified <see cref="Tile"/>.
        /// </summary>
        public class AddMeeplesMove : Move
        {
            private readonly Point point;

            /// <summary>
            /// Initializes a new instance of the <see cref="AddMeeplesMove"/> class.
            /// </summary>
            /// <param name="state0">The <see cref="GameState"/> that this move is based on.</param>
            /// <param name="point">The <see cref="Point"/> where the <see cref="Meeple">Meeples</see> will be added.</param>
            public AddMeeplesMove(GameState state0, Point point)
                : base(state0, state0.ActivePlayer)
            {
                this.point = point;
            }

            /// <summary>
            /// Gets the <see cref="Point"/> where the <see cref="Meeple">Meeples</see> will be added.
            /// </summary>
            public Point Point
            {
                get { return this.point; }
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return string.Format("Draw {0} Meeples and place at {1}", Math.Min(this.State.Bag.Count, 3), this.point);
            }

            internal override GameState Apply(GameState state0)
            {
                var player = state0.ActivePlayer;

                ImmutableList<Meeple> dealt;
                var newBag = state0.Bag;
                newBag = newBag.Deal(3, out dealt);

                return state0.With(
                    bag: newBag,
                    sultanate: state0.Sultanate.SetItem(this.point, state0.Sultanate[this.point].With(meeples: new EnumCollection<Meeple>(dealt))));
            }
        }
    }
}
