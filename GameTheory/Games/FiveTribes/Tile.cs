namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    public abstract class Tile
    {
        private readonly TileColor color;
        private readonly int value;

        protected Tile(int value, TileColor color)
        {
            this.value = value;
            this.color = color;
        }

        public TileColor Color
        {
            get { return this.color; }
        }

        public int Value
        {
            get { return this.value; }
        }

        public virtual IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            yield return new ChangePhaseMove(state, "Skip Tile Action", Phase.CleanUp);
        }

        public class BigMarket : Tile
        {
            public const int FirstN = 6;
            public const int Gold = 6;
            public static readonly BigMarket Instance = new BigMarket();

            private BigMarket()
                : base(4, TileColor.Red)
            {
            }

            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                if (state0.VisibleResources.Count > 0)
                {
                    var moves = Cost.Gold(state0, Gold, s => s, s1 => from i in Enumerable.Range(0, Math.Min(FirstN, s1.VisibleResources.Count))
                                                                      select new TakeResourceMove(s1, i, s2 =>
                                                                      {
                                                                          if (s2.VisibleResources.Count >= 1)
                                                                          {
                                                                              return s2.WithMoves(s3 => Enumerable.Concat(
                                                                                                         from j in Enumerable.Range(0, Math.Min(FirstN - 1, s3.VisibleResources.Count))
                                                                                                         select new TakeResourceMove(s3, j, s4 => s4.With(phase: Phase.CleanUp)),
                                                                                                         new Move[] { new ChangePhaseMove(s3, "Skip second resource", Phase.CleanUp) }));
                                                                          }
                                                                          else
                                                                          {
                                                                              return s2.With(phase: Phase.CleanUp);
                                                                          }
                                                                      }));

                    foreach (var m in moves) yield return m;
                }

                foreach (var m in base.GetTileActionMoves(state0)) yield return m;
            }
        }

        public class Oasis : Tile
        {
            public static readonly Oasis Instance = new Oasis();

            private Oasis()
                : base(8, TileColor.Red)
            {
            }

            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                yield return new PlacePalmTreeMove(state0, state0.LastPoint, s1 => s1.With(phase: Phase.CleanUp));
            }
        }

        public class SacredPlace : Tile
        {
            public SacredPlace(int value)
                : base(value, TileColor.Blue)
            {
            }

            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                if (state0.VisibleDjinns.Count >= 1)
                {
                    var moves = Cost.OneElderPlusOneElderOrOneSlave(state0, s => s, s1 => from i in Enumerable.Range(0, s1.VisibleDjinns.Count)
                                                                                          select new TakeDjinnMove(s1, i, s2 => s2.With(phase: Phase.CleanUp)));

                    foreach (var move in moves) yield return move;
                }

                foreach (var baseMove in base.GetTileActionMoves(state0)) yield return baseMove;
            }
        }

        public class SmallMarket : Tile
        {
            public const int FirstN = 3;
            public const int Gold = 3;
            public static readonly SmallMarket Instance = new SmallMarket();

            private SmallMarket()
                : base(6, TileColor.Red)
            {
            }

            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                if (state0.VisibleResources.Count >= 1)
                {
                    var moves = Cost.Gold(state0, Gold, s => s, s1 => from i in Enumerable.Range(0, Math.Min(FirstN, s1.VisibleResources.Count))
                                                                      select new TakeResourceMove(s1, i, s2 => s2.With(phase: Phase.CleanUp)));

                    foreach (var m in moves) yield return m;
                }

                foreach (var m in base.GetTileActionMoves(state0)) yield return m;
            }
        }

        public class Village : Tile
        {
            public static readonly Village Instance = new Village();

            private Village()
                : base(5, TileColor.Blue)
            {
            }

            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                yield return new PlacePalaceMove(state0, state0.LastPoint, s1 => s1.With(phase: Phase.CleanUp));
            }
        }
    }
}
