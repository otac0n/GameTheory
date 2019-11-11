// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Console
{
    using System;
    using System.Linq;
    using GameTheory.ConsoleRunner.Shared;
    using GameTheory.Games.SevenDragons.Cards;

    /// <summary>
    /// Provides a console renderer for the game of <see cref="GameState">Seven Dragons</see>.
    /// </summary>
    public class SevenDragonsConsoleRenderer : ConsoleRendererBase<GameState, Move>
    {
        /// <inheritdoc />
        public override void Show(GameState state, PlayerToken playerToken = null)
        {
            var writer = this.MakeRenderTokenWriter(state);
            var extents = state.Table.Keys.Aggregate(
                new { MinX = int.MaxValue, MaxX = int.MinValue, MinY = int.MaxValue, MaxY = int.MinValue },
                (value, key) => new { MinX = Math.Min(value.MinX, key.X), MaxX = Math.Max(value.MaxX, key.X), MinY = Math.Min(value.MinY, key.Y), MaxY = Math.Max(value.MaxY, key.Y) });

            for (var y = extents.MinY; y <= extents.MaxY; y++)
            {
                for (var v = 0; v < DragonCard.Grid.Height; v++)
                {
                    for (var x = extents.MinX; x <= extents.MaxX; x++)
                    {
                        if (state.Table.TryGetValue(new Point(x, y), out DragonCard card))
                        {
                            for (var u = 0; u < DragonCard.Grid.Width; u++)
                            {
                                ConsoleInteraction.WithColor(GetConsoleColor(card.Colors[DragonCard.Grid.IndexOf(u, v)]), () =>
                                {
                                    Console.Write(BoxDrawing.FullBlock);
                                });
                            }
                        }
                        else
                        {
                            for (var w = 0; w < DragonCard.Grid.Width; w++)
                            {
                                Console.Write(BoxDrawing.Space);
                            }
                        }

                        if (x < extents.MaxX)
                        {
                            Console.Write(BoxDrawing.Space);
                        }
                    }

                    Console.WriteLine();
                }

                if (playerToken != null || y < extents.MaxY)
                {
                    Console.WriteLine();
                }
            }

            if (playerToken != null)
            {
                var inventory = state.Inventories[state.InventoryMap[playerToken]];

                writer.WriteLine("{0}:", Resources.Goal);
                writer.WriteLine("    {0}", inventory.Goal);

                writer.WriteLine("{0}:", Resources.Hand);

                for (var v = 0; v < DragonCard.Grid.Height; v++)
                {
                    for (var i = 0; i < inventory.Hand.Count; i++)
                    {
                        var card = inventory.Hand[i];
                        if (card is DragonCard dragonCard)
                        {
                            for (var u = 0; u < DragonCard.Grid.Width; u++)
                            {
                                ConsoleInteraction.WithColor(GetConsoleColor(dragonCard.Colors[DragonCard.Grid.IndexOf(u, v)]), () =>
                                {
                                    Console.Write(BoxDrawing.FullBlock);
                                });
                            }
                        }
                        else
                        {
                            var name = card.GetType().Name;
                            ConsoleInteraction.WithColor(GetConsoleColor(((ActionCard)card).Color), () =>
                            {
                                Console.Write(name.Substring(DragonCard.Grid.Width * v, DragonCard.Grid.Width));
                            });
                        }

                        if (i < inventory.Hand.Count - 1)
                        {
                            Console.Write(' ');
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        /// <inheritdoc/>
        protected override void RenderToken(GameState state, object token)
        {
            if (token is DragonCard dragonCard)
            {
                var colorA = GetConsoleColor(dragonCard.Colors[0]);
                var colorB = GetConsoleColor(dragonCard.Colors[1]);
                var colorC = GetConsoleColor(dragonCard.Colors[2]);
                var colorD = GetConsoleColor(dragonCard.Colors[3]);
                ConsoleInteraction.WithColor(colorA, colorC, () =>
                {
                    Console.Write(BoxDrawing.UpperHalfBlock);
                });
                ConsoleInteraction.WithColor(colorB, colorD, () =>
                {
                    Console.Write(BoxDrawing.UpperHalfBlock);
                });
            }
            else if (token is ActionCard actionCard)
            {
                ConsoleInteraction.WithColor(GetConsoleColor(actionCard.Color), () =>
                {
                    base.RenderToken(state, token);
                });
            }
            else if (token is Color color)
            {
                ConsoleInteraction.WithColor(GetConsoleColor(color), () =>
                {
                    Console.Write(Resources.ResourceManager.GetEnumString(color));
                });
            }
            else
            {
                base.RenderToken(state, token);
            }
        }

        private static ConsoleColor GetConsoleColor(Color color)
        {
            switch (color)
            {
                case Color.Red:
                    return ConsoleColor.Red;

                case Color.Gold:
                    return ConsoleColor.Yellow;

                case Color.Blue:
                    return ConsoleColor.Blue;

                case Color.Green:
                    return ConsoleColor.Green;

                case Color.Black:
                    return ConsoleColor.DarkGray;

                case Color.Silver:
                    return ConsoleColor.White;

                case Color.Rainbow:
                default:
                    return ConsoleColor.Magenta;
            }
        }
    }
}
