// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Strategies
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.Strategies;
    using NUnit.Framework;

    [TestFixture]
    public class NullStrategyTests
    {
        [Test]
        public async Task GetMove_Always_ReturnsNoMove()
        {
            var gameState = new TestGameState();
            using (var strategy = new NullStrategy<TestGameState.Move>())
            {
                var maybeMove = await strategy.ChooseMove(gameState, gameState.Players[0], CancellationToken.None);
                Assert.That(maybeMove.HasValue, Is.False);
            }
        }

        public class TestGameState : IGameState<TestGameState.Move>
        {
            private readonly ReadOnlyCollection<PlayerToken> players;

            public TestGameState()
            {
                this.players = new List<PlayerToken> { new PlayerToken() }.AsReadOnly();
            }

            public IReadOnlyList<PlayerToken> Players => this.players;

            public IReadOnlyCollection<Move> GetAvailableMoves()
            {
                return this.players.Select(p => new Move(p)).ToList().AsReadOnly();
            }

            public IReadOnlyCollection<PlayerToken> GetWinners()
            {
                return new List<PlayerToken>().AsReadOnly();
            }

            public IGameState<Move> MakeMove(Move move)
            {
                return this;
            }

            public IGameState<Move> GetView(PlayerToken playerToken)
            {
                return this;
            }

            public class Move : IMove
            {
                public Move(PlayerToken playerToken)
                {
                    this.PlayerToken = playerToken;
                }

                public PlayerToken PlayerToken { get; }
            }
        }
    }
}
