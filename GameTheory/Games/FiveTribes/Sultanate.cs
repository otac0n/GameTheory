// -----------------------------------------------------------------------
// <copyright file="Sultanate.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Provides utility functions for working with the Sultanate, a collection of <see cref="Square">Squares</see>.
    /// </summary>
    public static class Sultanate
    {
        /// <summary>
        /// The height of the Sultanate.
        /// </summary>
        public const int Height = 5;

        /// <summary>
        /// The minimum number of Meeples such that you can make a loop with them.
        /// </summary>
        public const int RequiredForLoop = 5;

        /// <summary>
        /// The width of the Sultanate.
        /// </summary>
        public const int Width = 6;

        private static readonly ImmutableList<ImmutableList<Point>> SquarePoints;
        private static readonly Dictionary<Tuple<Point, Direction, int>, ImmutableHashSet<Point>> Storage = new Dictionary<Tuple<Point, Direction, int>, ImmutableHashSet<Point>>();

        static Sultanate()
        {
            SquarePoints = Enumerable.Range(0, Width * Height).Select(i =>
            {
                var points = ImmutableList.CreateBuilder<Point>();

                var px = i % Width;
                var py = i / Width;
                for (var y = -1; y <= 1; y++)
                {
                    for (var x = -1; x <= 1; x++)
                    {
                        var dpx = px + x;
                        var dpy = py + y;

                        if (dpx >= 0 && dpx < Width &&
                            dpy >= 0 && dpy < Height)
                        {
                            points.Add(new Point(dpx, dpy));
                        }
                    }
                }

                return points.ToImmutable();
            }).ToImmutableList();
        }

        /// <summary>
        /// Gets a direction from two adjacent Points.
        /// </summary>
        /// <param name="from">The source point.</param>
        /// <param name="to">The destination point.</param>
        /// <returns>The direction traveled from the source point to the destination point.</returns>
        public static Direction GetDirection(Point from, Point to)
        {
            return to.Y < from.Y ? Direction.Up :
                   to.X > from.X ? Direction.Right :
                   to.Y > from.Y ? Direction.Down :
                   to.X < from.X ? Direction.Left :
                   Direction.None;
        }

        /// <summary>
        /// Gets all of the Point and Meeple combinations that represent legal moves given the specified values.
        /// </summary>
        /// <param name="sultanate">The sultanate.</param>
        /// <param name="point">The most recently touched point.</param>
        /// <param name="incomingDirection">The direction that was most recently traveled to get to <see cref="point"/>, or <see cref="Direction.None"/> if it was picked up.</param>
        /// <param name="inHand">The meeples in hand.</param>
        /// <returns>A sequence containing all legal combinations of Point and Meeple.</returns>
        public static IEnumerable<Tuple<Meeple, Point>> GetMoves(this IList<Square> sultanate, Point point, Direction incomingDirection, EnumCollection<Meeple> inHand)
        {
            var count = inHand.Count;
            var potentialDrops = Sultanate.FindDestinations(point, incomingDirection, 1);

            if (count == 1)
            {
                var only = inHand.First();
                foreach (var drop in potentialDrops)
                {
                    if (sultanate[drop].Meeples.Contains(only))
                    {
                        yield return Tuple.Create(only, drop);
                    }
                }

                yield break;
            }

            var dupeCount = inHand.Keys.Count(k => inHand[k] > 1);

            var hasMoreThanEnoughForLoop = count > RequiredForLoop;
            var includeNonDupes = hasMoreThanEnoughForLoop && dupeCount >= 1;

            foreach (var drop in potentialDrops)
            {
                IEnumerable<Meeple> availableMoves;

                if (hasMoreThanEnoughForLoop && dupeCount >= 2)
                {
                    availableMoves = inHand.Keys;
                }
                else
                {
                    var destinationSquares = Sultanate.FindDestinations(drop, GetDirection(from: point, to: drop), count - 1);

                    var includeAllDupes = destinationSquares.Contains(drop);

                    if (includeAllDupes && includeNonDupes)
                    {
                        availableMoves = inHand.Keys;
                    }
                    else
                    {
                        var availableDestinationColors = destinationSquares
                            .SelectMany(p => sultanate[p].Meeples)
                            .Intersect(inHand.Keys)
                            .Take(2)
                            .ToList();

                        if (availableDestinationColors.Count > 1)
                        {
                            availableMoves = inHand.Keys;
                        }
                        else if (availableDestinationColors.Count == 1)
                        {
                            var only = availableDestinationColors.First();
                            if (inHand[only] > 1)
                            {
                                availableMoves = inHand.Keys;
                            }
                            else
                            {
                                if (includeAllDupes || includeNonDupes)
                                {
                                    availableMoves = inHand.Keys.Where(k => k != only || includeAllDupes == (inHand[k] > 1));
                                }
                                else
                                {
                                    availableMoves = inHand.Keys.Where(k => k != only);
                                }
                            }
                        }
                        else
                        {
                            availableMoves = Enumerable.Empty<Meeple>();
                        }
                    }
                }

                foreach (var m in availableMoves)
                {
                    yield return Tuple.Create(m, drop);
                }
            }
        }

        /// <summary>
        /// Gets all of the squares that have legal moves if they were to be picked up.
        /// </summary>
        /// <param name="sultanate">The sultanate.</param>
        /// <returns>All of the legal pick ups.</returns>
        public static IEnumerable<Point> GetPickUps(this ImmutableList<Square> sultanate)
        {
            for (var i = 0; i < Width * Height; i++)
            {
                var inHand = sultanate[i].Meeples;
                var count = inHand.Count;
                if (count > 0)
                {
                    var newSultanate = sultanate.SetItem(i, sultanate[i].With(meeples: EnumCollection<Meeple>.Empty));

                    var hasMoves = (count >= RequiredForLoop && count >= GameState.TribesCount + 1)
                                || newSultanate.GetMoves(i, Direction.None, inHand).Any();
                    if (hasMoves)
                    {
                        yield return i;
                    }
                }
            }
        }

        /// <summary>
        /// Gets points within the specified distance of the specified point.
        /// </summary>
        /// <param name="point">The starting point.</param>
        /// <param name="distance">The inclusive maximum distance away, in number of moves.</param>
        /// <returns>The points that are within the specified distance.</returns>
        public static IEnumerable<Point> GetPointsWithin(Point point, int distance)
        {
            var x1 = point.X;
            var y1 = point.Y;

            for (var i = 0; i < Width * Height; i++)
            {
                var x2 = i % Width;
                var y2 = i / Width;

                if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) <= distance)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Get the square containing all of the points adjacent to the specified point.
        /// </summary>
        /// <param name="point">The center point of the requested square.</param>
        /// <returns>All of the points within the requested square.</returns>
        public static ImmutableList<Point> GetSquarePoints(Point point)
        {
            return SquarePoints[point];
        }

        private static ImmutableHashSet<Point> FindDestinations(Point point, Direction incomingDirection, int meeples)
        {
            var key = Tuple.Create(point, incomingDirection, meeples);
            ImmutableHashSet<Point> result;
            return Storage.TryGetValue(key, out result) ? result : Storage[key] = FindDestinationsImpl(point, incomingDirection, meeples);
        }

        private static ImmutableHashSet<Point> FindDestinationsImpl(Point point, Direction incomingDirection, int meeples)
        {
            if (meeples == 0)
            {
                return ImmutableHashSet.Create(point);
            }

            var destinations = ImmutableHashSet<Point>.Empty;

            if (incomingDirection != Direction.Down && point.Y > 0)
            {
                destinations = destinations.Union(FindDestinations(point - Width, Direction.Up, meeples - 1));
            }

            if (incomingDirection != Direction.Left && point.X < Width - 1)
            {
                destinations = destinations.Union(FindDestinations(point + 1, Direction.Right, meeples - 1));
            }

            if (incomingDirection != Direction.Up && point.Y < Height - 1)
            {
                destinations = destinations.Union(FindDestinations(point + Width, Direction.Down, meeples - 1));
            }

            if (incomingDirection != Direction.Right && point.X > 0)
            {
                destinations = destinations.Union(FindDestinations(point - 1, Direction.Left, meeples - 1));
            }

            return destinations;
        }
    }
}
