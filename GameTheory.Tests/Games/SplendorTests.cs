// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.Splendor;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class SplendorTests
    {
        [Test(Description = "With 2 players: Remove 3 tokens of each gem color (there should be only 4 of each remaining). With 3 players: Remove 3 tokens of each gem color (there should be only 5 of each remaining).")]
        [TestCase(2, 4)]
        [TestCase(3, 5)]
        [TestCase(4, 7)]
        public void ctor_Always_AddGemsAndFiveGoldeJokers(int players, int gemTokens)
        {
            var state = new GameState(players);

            var tokens = state.Tokens.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());

            Assert.That(tokens[Token.Diamond], Is.EqualTo(gemTokens));
            Assert.That(tokens[Token.Emerald], Is.EqualTo(gemTokens));
            Assert.That(tokens[Token.Onyx], Is.EqualTo(gemTokens));
            Assert.That(tokens[Token.Ruby], Is.EqualTo(gemTokens));
            Assert.That(tokens[Token.Sapphire], Is.EqualTo(gemTokens));
            Assert.That(tokens[Token.GoldJoker], Is.EqualTo(5));
        }

        [Test(Description = "Shuffle the noble tiles and reveal as many of them as there are players plus one (example: 5 tiles for a 4 player game).")]
        public void ctor_Always_AddsOneMoreNobleThanPlayers([Values(2, 3, 4)] int players)
        {
            var state = new GameState(players);

            Assert.That(state.Nobles.Count, Is.EqualTo(players + 1));
        }

        [Test(Description = "Shuffle each development card deck separately, and the place them in a column in the middle of the table in increasing order from bottom to top. Then reveal 4 cards from each level.")]
        public void ctor_Always_DealsFourDevelopmentCardsFromEachDeck([Values(2, 3, 4)] int players)
        {
            var state = new GameState(players);

            Assert.That(state.DevelopmentTracks.Length, Is.EqualTo(3));
            Assert.That(state.DevelopmentTracks.Select(t => t.Length), Is.All.EqualTo(4));
        }

        [Test(Description = "The player who has the highest number of prestige points is declared the winner. In case of a tie, the player who has purchased the fewest development cards wins.")]
        public void GetWinners_AfterAGameHasBeenPlayed_ReturnsThePlayersWithTheHighestScoreWithTheFewestCards()
        {
            var endState = (GameState)GameUtilities.PlayGame(
                new GameState(2),
                p => new RandomPlayer<Move>(p),
                (state, move) => Console.WriteLine("{0}: {1}", state.GetPlayerName(move.PlayerToken), move)).Result;

            var highestScore = endState.Players.Max(p => endState.GetScore(p));
            var playersWithHighestScore = endState.Players.Where(p => endState.GetScore(p) == highestScore);
            var fewestCards = playersWithHighestScore.Min(p => endState.Inventory[p].DevelopmentCards.Count);
            var winners = endState.GetWinners();

            string formatWinners(IEnumerable<PlayerToken> players) => string.Join(", ", players.Select(player => endState.GetPlayerName(player)).OrderBy(n => n));
            Assert.That(formatWinners(winners), Is.EqualTo(formatWinners(playersWithHighestScore.Where(p => endState.Inventory[p].DevelopmentCards.Count == fewestCards))));
        }
    }
}
