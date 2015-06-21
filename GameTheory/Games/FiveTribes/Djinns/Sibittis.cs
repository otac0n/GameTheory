namespace GameTheory.Games.FiveTribes.Djinns
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class Sibittis : Djinn.PayPerActionDjinnBase
    {
        public static readonly Sibittis Instance = new Sibittis();

        private Sibittis()
            : base(4, Cost.OneElderPlusOneElderOrOneSlave)
        {
        }

        protected override bool CanGetMoves(GameState state)
        {
            return base.CanGetMoves(state) && (state.DjinnPile.Count + state.DjinnDiscards.Count) >= 1;
        }

        protected override IEnumerable<Move> GetAppliedCostMoves(GameState state0)
        {
            yield return new DrawDjinnsMove(state0);
        }

        public class DrawDjinnsMove : Move
        {
            public DrawDjinnsMove(GameState state0)
                : base(state0, state0.ActivePlayer, s1 =>
                {
                    var toDraw = GetDrawCount(s1);

                    ImmutableList<Djinn> dealt;
                    var newDjinnDiscards = s1.DjinnDiscards;
                    var newDjinnPile = s1.DjinnPile.Deal(toDraw, out dealt, ref newDjinnDiscards);

                    var s2 = s1.With(
                        djinnPile: newDjinnPile,
                        djinnDiscards: newDjinnDiscards);

                    return s2.WithMoves(s3 => Enumerable.Range(0, toDraw).Select(i => new TakeDealtDjinnMove(s3, dealt, i)));
                })
            {
            }

            public override string ToString()
            {
                return string.Format("Draw {0} Djinns", GetDrawCount(this.State));
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
                : base(state0, state0.ActivePlayer, s4 =>
                {
                    var player = s4.ActivePlayer;
                    var inventory = s4.Inventory[player];

                    return s4.With(
                        djinnDiscards: s4.DjinnDiscards.AddRange(dealt.RemoveAt(index)),
                        inventory: s4.Inventory.SetItem(player, inventory.With(djinns: inventory.Djinns.Add(dealt[index]))));
                })
            {
                this.dealt = dealt;
                this.index = index;
            }

            public override string ToString()
            {
                return string.Format("Take {0}", this.dealt[this.index]);
            }
        }
    }
}
