// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared
{
    using System;
    using System.Collections.Generic;

    public static class ConsoleInteraction
    {
        private static readonly IReadOnlyList<ConsoleColor> PlayerColors = new List<ConsoleColor>
        {
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed,
        }.AsReadOnly();

        public static ConsoleColor GetPlayerColor<TGameState, TMove>(TGameState state, PlayerToken playerToken)
            where TGameState : IGameState<TMove>
            where TMove : IMove
        {
            var i = 0;
            foreach (var player in state.Players)
            {
                if (player == playerToken)
                {
                    return PlayerColors[i % PlayerColors.Count];
                }

                i++;
            }

            return ConsoleColor.White;
        }

        public static void WithColor(ConsoleColor color, Action action)
        {
            var originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                action();
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        public static void WithColor(ConsoleColor foreground, ConsoleColor background, Action action)
        {
            var originalForeground = Console.ForegroundColor;
            var originalBackground = Console.BackgroundColor;
            try
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                action();
            }
            finally
            {
                Console.ForegroundColor = originalForeground;
                Console.BackgroundColor = originalBackground;
            }
        }
    }
}
