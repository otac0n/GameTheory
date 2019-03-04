// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides methods for rendering and formatting game objects.
    /// </summary>
    public static class FormatUtilities
    {
        /// <summary>
        /// Builds a list of format tokens from the specified arguments.
        /// </summary>
        /// <param name="items">A list of format tokens. Entries that are <c>null</c> will be excluded.</param>
        /// <returns>The list of format tokens.</returns>
        public static IList<object> Build(params object[] items)
        {
            var result = new List<object>(items.Length);

            foreach (var item in items)
            {
                if (item is IEnumerable<object> enumerable)
                {
                    foreach (var subItem in enumerable)
                    {
                        result.Add(subItem);
                    }
                }
                else
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Recursively retrieves the flattened collection of format tokens.
        /// </summary>
        /// <param name="this">The <see cref="ITokenFormattable"/> being formatted.</param>
        /// <returns>An enumerable collection containing atomic format tokens.</returns>
        public static IEnumerable<object> FlattenFormatTokens(this ITokenFormattable @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return FlattenFormatTokens(@this.FormatTokens);
        }

        /// <summary>
        /// Recursively retrieves the flattened collection of format tokens.
        /// </summary>
        /// <param name="formatTokens">The collection of tokens being traversed.</param>
        /// <returns>An enumerable collection containing atomic format tokens.</returns>
        public static IEnumerable<object> FlattenFormatTokens(this IList<object> formatTokens)
        {
            foreach (var token in formatTokens)
            {
                if (token is ITokenFormattable formattable)
                {
                    foreach (var subToken in formattable.FlattenFormatTokens())
                    {
                        yield return subToken;
                    }
                }
                else
                {
                    yield return token;
                }
            }
        }

        /// <summary>
        /// Returns the list of format tokens representing a list.
        /// </summary>
        /// <param name="items">The items in the list that will be separated.</param>
        /// <returns>The format tokens representing the list.</returns>
        public static IList<object> FormatList(IEnumerable<object> items) => FormatList(items.ToList());

        /// <summary>
        /// Returns the list of format tokens representing a list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="items">The items in the list that will be separated.</param>
        /// <returns>The format tokens representing the list.</returns>
        public static IList<object> FormatList<T>(IList<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            string sep;
            switch (items.Count)
            {
                case 0:
                    return new object[0];

                case 1:
                    return new object[] { items[0] };

                case 2:
                    sep = SharedResources.ListItemSeparatorPair;
                    break;

                default:
                    sep = SharedResources.ListItemSeparatorLastElement;
                    break;
            }

            var result = new object[items.Count * 2 - 1];
            var i = items.Count - 1;
            var ix = result.Length - 1;
            result[ix--] = items[i--];
            result[ix--] = sep;

            for (; i >= 1;)
            {
                result[ix--] = items[i--];
                result[ix--] = SharedResources.ListItemSeparator;
            }

            result[ix--] = items[i];

            return result;
        }

        /// <summary>
        /// Gets a player name for display.
        /// </summary>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The game state.</param>
        /// <param name="playerToken">The player to search for.</param>
        /// <returns>A name representing the specified player token.</returns>
        public static string GetPlayerName<TMove>(this IGameState<TMove> state, PlayerToken playerToken)
            where TMove : IMove =>
            string.Format(SharedResources.PlayerName, state.GetPlayerNumber(playerToken));

        /// <summary>
        /// Provides a replacement for conditional expressions that avoids explicit casting.
        /// </summary>
        /// <param name="predicate">A value indicating whether or not to return the result of <paramref name="getFormat"/>.</param>
        /// <param name="getFormat">The function that will be invoked conditionally.</param>
        /// <returns>The result of the <paramref name="getFormat"/> function, if <paramref name="predicate"/> is true; <c>null</c>, otherwise.</returns>
        public static object If(bool predicate, Func<object> getFormat) => predicate ? getFormat() : null;

        /// <summary>
        /// Parses a composite format string to a collection of format tokens.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>An enumerable collection containing format tokens.</returns>
        public static IList<object> ParseStringFormat(string format, params object[] args)
        {
            var result = new List<object>();

            foreach (Match match in Regex.Matches(format, @"(^|\G)([^{]+|{(?<index>\d+)})"))
            {
                var index = match.Groups["index"];
                var item = index.Success
                    ? args[int.Parse(index.Value)]
                    : match.Value;

                if (item is IEnumerable<object> enumerable)
                {
                    foreach (var subItem in enumerable)
                    {
                        result.Add(subItem);
                    }
                }
                else
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}
