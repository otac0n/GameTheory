// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using System.Linq;
    using Games.Splendor;

    /// <summary>
    /// Implements a console renderer for the game of <see cref="GameState">Splendor</see>.
    /// </summary>
    public class SplendorConsoleRenderer : IConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public void Show(IGameState<Move> state, PlayerToken playerToken)
        {
            var gameState = (GameState)state;

            if (gameState.Nobles.Count > 0)
            {
                Console.WriteLine("Nobles:");
                for (int i = 0; i < gameState.Nobles.Count; i++)
                {
                    Console.WriteLine("  [requires: {0}]", gameState.Nobles[i].RequiredBonuses);
                }

                Console.WriteLine();
            }

            for (var i = gameState.DevelopmentTracks.Length - 1; i >= 0; i--)
            {
                var track = gameState.DevelopmentTracks[i];
                Console.WriteLine("Track {0}: [remaining: {1}]", i + 1, gameState.DevelopmentDecks[i].Count);
                for (var j = 0; j < track.Length; j++)
                {
                    var card = track[j];
                    if (card == null)
                    {
                        Console.WriteLine("  [empty]");
                    }
                    else
                    {
                        Console.WriteLine("  {0} [cost: {1}]", card, card.Cost);
                    }
                }
            }

            Console.WriteLine();

            Console.WriteLine("Available Tokens:");
            Console.WriteLine("  {0}", gameState.Tokens);

            Console.WriteLine();

            if (playerToken != null)
            {
                Console.WriteLine("{0} Inventory:", gameState.GetPlayerName(playerToken));

                var inventory = gameState.Inventory[playerToken];

                Console.WriteLine("  Prestige: {0}", gameState.GetScore(playerToken));

                if (inventory.Nobles.Count > 0)
                {
                    Console.WriteLine("  Nobles: {0} [bonus: {1}]", inventory.Nobles.Count, inventory.Nobles.Count * Noble.PrestigeBonus);
                }

                if (inventory.Tokens.Count > 0)
                {
                    Console.WriteLine("  Tokens: {0}", inventory.Tokens);
                }

                if (inventory.Hand.Count > 0)
                {
                    Console.WriteLine("  Hand:");
                    foreach (var card in inventory.Hand)
                    {
                        Console.WriteLine("    {0} [cost: {1}]", card, card.Cost);
                    }
                }

                if (inventory.DevelopmentCards.Count > 0)
                {
                    Console.WriteLine("  Development Cards:");
                    foreach (var card in inventory.DevelopmentCards.OrderBy(c => c.Bonus).ThenByDescending(c => c.Prestige))
                    {
                        Console.WriteLine("    {0}", card);
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
