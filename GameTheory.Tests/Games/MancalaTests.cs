// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Games
{
    using GameTheory.Games.Mancala;
    using NUnit.Framework;

    [TestFixture]
    public class MancalaTests
    {
        private static string p(GameState state, PlayerToken player)
        {
            return ((char)('A' + state.Players.IndexOf(player))).ToString();
        }
    }
}
