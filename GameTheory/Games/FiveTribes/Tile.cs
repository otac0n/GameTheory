// -----------------------------------------------------------------------
// <copyright file="Tile.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.FiveTribes.Moves;

    /// <summary>
    /// Represents a tile in game of Five Tribes.
    /// </summary>
    public abstract class Tile
    {
        private readonly TileColor color;
        private readonly int value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="value">The value of the <see cref="Tile"/>, in Victory Points (VP).</param>
        /// <param name="color">The color of the <see cref="Tile"/>.</param>
        protected Tile(int value, TileColor color)
        {
            this.value = value;
            this.color = color;
        }

        /// <summary>
        /// Gets the color of the <see cref="Tile"/>.
        /// </summary>
        public TileColor Color
        {
            get { return this.color; }
        }

        /// <summary>
        /// Gets the value of the <see cref="Tile"/>, in Victory Points (VP).
        /// </summary>
        public int Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Generates moves for the specified <see cref="GameState"/>.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> for which <see cref="Move">Moves</see> are being generated.</param>
        /// <returns>The <see cref="Move">Moves</see> provided by the <see cref="Tile"/>.</returns>
        public virtual IEnumerable<Move> GetTileActionMoves(GameState state)
        {
            yield return new ChangePhaseMove(state, "Skip Tile Action", Phase.CleanUp);
        }

        /// <summary>
        /// Represents a Big Market tile.
        /// </summary>
        public class BigMarket : Tile
        {
            /// <summary>
            /// The number of <see cref="Resource">Resources</see> the player can choose from, starting from the beginning.
            /// </summary>
            public const int FirstN = 6;

            /// <summary>
            /// The cost of using the <see cref="BigMarket"/> to purchase two <see cref="Resource">Resources</see>.
            /// </summary>
            public const int Gold = 6;

            /// <summary>
            /// The singleton instance of <see cref="BigMarket"/>.
            /// </summary>
            public static readonly BigMarket Instance = new BigMarket();

            private BigMarket()
                : base(4, TileColor.Red)
            {
            }

            /// <inheritdoc />
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
                                                                                                         new Move[]
                                                                                                         {
                                                                                                             new ChangePhaseMove(s3, "Skip second resource", Phase.CleanUp)
                                                                                                         }));
                                                                          }
                                                                          else
                                                                          {
                                                                              return s2.With(phase: Phase.CleanUp);
                                                                          }
                                                                      }));

                    foreach (var m in moves)
                    {
                        yield return m;
                    }
                }

                foreach (var m in base.GetTileActionMoves(state0))
                {
                    yield return m;
                }
            }
        }

        /// <summary>
        /// Represents an Oasis tile.
        /// </summary>
        public class Oasis : Tile
        {
            /// <summary>
            /// The singleton instance of <see cref="Oasis"/>.
            /// </summary>
            public static readonly Oasis Instance = new Oasis();

            private Oasis()
                : base(8, TileColor.Red)
            {
            }

            /// <inheritdoc />
            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                yield return new PlacePalmTreeMove(state0, state0.LastPoint, s1 => s1.With(phase: Phase.CleanUp));
            }
        }

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
            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                if (state0.VisibleDjinns.Count >= 1)
                {
                    var moves = Cost.OneElderPlusOneElderOrOneSlave(state0, s => s, s1 => from i in Enumerable.Range(0, s1.VisibleDjinns.Count)
                                                                                          select new TakeDjinnMove(s1, i, s2 => s2.With(phase: Phase.CleanUp)));

                    foreach (var move in moves)
                    {
                        yield return move;
                    }
                }

                foreach (var baseMove in base.GetTileActionMoves(state0))
                {
                    yield return baseMove;
                }
            }
        }

        /// <summary>
        /// Represents a Small Market tile.
        /// </summary>
        public class SmallMarket : Tile
        {
            /// <summary>
            /// The number of <see cref="Resource">Resources</see> the player can choose from, starting from the beginning.
            /// </summary>
            public const int FirstN = 3;

            /// <summary>
            /// The cost of using the <see cref="SmallMarket"/> to purchase a <see cref="Resource"/>.
            /// </summary>
            public const int Gold = 3;

            /// <summary>
            /// The singleton instance of <see cref="SmallMarket"/>.
            /// </summary>
            public static readonly SmallMarket Instance = new SmallMarket();

            private SmallMarket()
                : base(6, TileColor.Red)
            {
            }

            /// <inheritdoc />
            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                if (state0.VisibleResources.Count >= 1)
                {
                    var moves = Cost.Gold(state0, Gold, s => s, s1 => from i in Enumerable.Range(0, Math.Min(FirstN, s1.VisibleResources.Count))
                                                                      select new TakeResourceMove(s1, i, s2 => s2.With(phase: Phase.CleanUp)));

                    foreach (var m in moves)
                    {
                        yield return m;
                    }
                }

                foreach (var m in base.GetTileActionMoves(state0))
                {
                    yield return m;
                }
            }
        }

        /// <summary>
        /// Represents a Village tile.
        /// </summary>
        public class Village : Tile
        {
            /// <summary>
            /// The singleton instance of <see cref="Village"/>.
            /// </summary>
            public static readonly Village Instance = new Village();

            private Village()
                : base(5, TileColor.Blue)
            {
            }

            /// <inheritdoc />
            public override IEnumerable<Move> GetTileActionMoves(GameState state0)
            {
                yield return new PlacePalaceMove(state0, state0.LastPoint, s1 => s1.With(phase: Phase.CleanUp));
            }
        }
    }
}
