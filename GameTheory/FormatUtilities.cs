// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides methods for rendering and formatting game objects.
    /// </summary>
    public static class FormatUtilities
    {
        /// <summary>
        /// Recursively retrieves the flattened collection of format tokens.
        /// </summary>
        /// <param name="this">The <see cref="ITokenFormattable"/> being formatted.</param>
        /// <returns>An enumerable collection containing atomic format tokens.</returns>
        public static IEnumerable<object> FlattenFormatTokens(this ITokenFormattable @this)
        {
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
        /// Gets a player name for display.
        /// </summary>
        /// <typeparam name="TMove">The type of object that represents a move in the game state.</typeparam>
        /// <param name="state">The game state.</param>
        /// <param name="playerToken">The player to search for.</param>
        /// <returns>A name representing the specified player token.</returns>
        public static string GetPlayerName<TMove>(this IGameState<TMove> state, PlayerToken playerToken)
            where TMove : IMove
        {
            return $"Player {state.GetPlayerNumber(playerToken)}";
        }

        /// <summary>
        /// Parses a composite format string to a collection of format tokens.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>An enumerable collection containing format tokens.</returns>
        public static IList<object> ParseStringFormat(string format, params object[] args)
        {
            return Regex.Matches(format, @"(^|\G)([^{]+|{(?<index>\d+)})").Cast<Match>().Select(m =>
            {
                var index = m.Groups["index"];
                return index.Success
                    ? args[int.Parse(index.Value)]
                    : m.Value;
            }).ToList();
        }
    }
}
