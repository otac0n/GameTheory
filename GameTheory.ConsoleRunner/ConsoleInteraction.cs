// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner
{
    using System;
    using System.Collections.Generic;

    internal static class ConsoleInteraction
    {
        public static T Choose<T>(IList<T> options, Func<T, string> skipMessage = null)
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

            List(options);
            var selection = Prompt(
                $"Please make a selection: [1-{options.Count}]",
                int.Parse,
                i => i > 0 && i <= options.Count ? null : $"Please choose a number between 1 and {options.Count} (inclusive).");

            return options[selection - 1];
        }

        public static void List<T>(IList<T> items, Func<T, string> toString = null)
        {
            toString = toString ?? new Func<T, string>(item => item?.ToString());
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {toString(items[i])}");
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
                    Console.WriteLine($"Input not recognized. {ex.Message}");
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
    }
}
