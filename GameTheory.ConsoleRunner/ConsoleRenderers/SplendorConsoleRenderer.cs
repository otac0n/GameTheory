// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.ConsoleRenderers
{
    using System;
    using System.Linq;
    using Games.Splendor;

    public class SplendorConsoleRenderer : IConsoleRenderer<Move>
    {
        /// <inheritdoc />
        public void Show(PlayerToken playerToken, IGameState<Move> state)
        {
            var gameState = (GameState)state;
            Console.WriteLine("Nobles:");
            for (int i = 0; i < gameState.Nobles.Count; i++)
            {
                Console.WriteLine("  [requires: {0}]", gameState.Nobles[i].RequiredBonuses);
            }

            Console.WriteLine();

            for (var i = gameState.DevelopmentTracks.Length - 1; i >= 0; i--)
            {
                var track = gameState.DevelopmentTracks[i];
                Console.WriteLine("Track {0}: [remaining: {1}]", i + 1, gameState.DevelopmentDecks[i].Count);
                for (var j = 0; j < track.Length; j++)
                {
                    var card = track[j];
                    Console.WriteLine("  {0} [cost: {1}]", card, card.Cost);
                }
            }

            Console.WriteLine();

            Console.WriteLine("Active Player Inventory:");

            var inventory = gameState.Inventory[gameState.ActivePlayer];

            Console.WriteLine("  Prestige: {0}", gameState.GetScore(gameState.ActivePlayer));

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
                foreach (var card in inventory.DevelopmentCards)
                {
                    Console.Write("    {0}", card);
                }
            }

            if (inventory.DevelopmentCards.Count > 0)
            {
                Console.WriteLine("  Development Cards:");
                foreach (var card in inventory.DevelopmentCards)
                {
                    Console.Write("    {0}", card);
                }
            }

            Console.WriteLine();
        }
    }
}
