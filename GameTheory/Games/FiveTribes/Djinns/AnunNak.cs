namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class AnunNak : Djinn.PayPerActionDjinnBase
    {
        public static readonly AnunNak Instance = new AnunNak();

        private AnunNak()
            : base(8, Cost.OneElderOrOneSlave)
        {
        }

        public override string Name
        {
            get { return "Anun-Nak"; }
        }

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

        public class AddMeeplesMove : Move
        {
            private readonly Point point;

            public AddMeeplesMove(GameState state0, Point point)
                : base(state0, state0.ActivePlayer, s1 =>
                {
                    var player = s1.ActivePlayer;

                    ImmutableList<Meeple> dealt;
                    var newBag = s1.Bag;
                    newBag = newBag.Deal(3, out dealt);

                    return s1.With(
                        bag: newBag,
                        sultanate: s1.Sultanate.SetItem(point, s1.Sultanate[point].With(meeples: new EnumCollection<Meeple>(dealt))));
                })
            {
                this.point = point;
            }

            public Point Point
            {
                get { return this.point; }
            }

            public override string ToString()
            {
                return string.Format("Draw {0} Meeples and place at {1}", Math.Min(this.State.Bag.Count, 3), this.point);
            }
        }
    }
}
