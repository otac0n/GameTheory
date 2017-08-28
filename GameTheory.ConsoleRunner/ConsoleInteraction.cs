// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using GameTheory.ConsoleRunner.Properties;

    internal static class ConsoleInteraction
    {
        private static readonly IReadOnlyList<ConsoleColor> PlayerColors = new List<ConsoleColor>
        {
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed,
        }.AsReadOnly();

        public static T Choose<T>(IList<T> options, CancellationToken? cancel = null, Action<T> render = null, Func<T, string> skipMessage = null)
        {
            if (options.Count == 0)
            {
                return default(T);
            }

            if (skipMessage != null && options.Count == 1)
            {
                var single = options[0];
                Console.WriteLine(skipMessage(single));
                return single;
            }

            Func<string, int> parse;
            if (cancel != null)
            {
                parse = s =>
                {
                    if (cancel.Value.IsCancellationRequested)
                    {
                        Console.WriteLine(Resources.InputDiscarded);
                        throw new OperationCanceledException();
                    }

                    return int.Parse(s);
                };
            }
            else
            {
                parse = int.Parse;
            }

            List(options, render);
            var selection = Prompt(
                string.Format(Resources.ListPrompt, options.Count),
                parse,
                i => i > 0 && i <= options.Count ? null : string.Format(Resources.InvalidListItem, options.Count));

            return options[selection - 1];
        }

        public static void List<T>(IList<T> items, Action<T> render = null)
        {
            render = render ?? new Action<T>(item => Console.Write(item?.ToString()));
            for (var i = 0; i < items.Count; i++)
            {
                Console.Write(Resources.ListItem, i + 1);
                render(items[i]);
                Console.WriteLine();
            }
        }

        public static string Prompt(string prompt, Func<string, string> validate = null)
        {
            return Prompt(prompt, x => x, validate);
        }

        public static T Prompt<T>(string prompt, Func<string, T> parse, Func<T, string> validate = null)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var line = Console.ReadLine();

                T value;
                try
                {
                    value = parse(line);
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException)
                {
                    Console.WriteLine(Resources.InvalidInput, ex.Message);
                    continue;
                }

                var errorMessage = validate?.Invoke(value);
                if (errorMessage != null)
                {
                    Console.WriteLine(errorMessage);
                    continue;
                }

                return value;
            }
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

        public static ConsoleColor GetPlayerColor<TMove>(IGameState<TMove> state, PlayerToken playerToken)
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
    }
}
