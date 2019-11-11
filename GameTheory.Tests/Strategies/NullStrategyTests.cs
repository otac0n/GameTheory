// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Strategies
{
    using System;
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
            var state = new TestGameState();
            using (var strategy = new NullStrategy<TestGameState, TestGameState.Move>())
            {
                var maybeMove = await strategy.ChooseMove(state, state.Players[0], CancellationToken.None);
                Assert.That(maybeMove.HasValue, Is.False);
            }
        }

        private class TestGameState : IGameState<TestGameState.Move>
        {
            private readonly Guid id = Guid.NewGuid();
            private readonly ReadOnlyCollection<PlayerToken> players;

            public TestGameState()
            {
                this.players = new List<PlayerToken> { new PlayerToken() }.AsReadOnly();
            }

            public IReadOnlyList<PlayerToken> Players => this.players;

            public int CompareTo(IGameState<Move> other)
            {
                if (object.ReferenceEquals(other, this))
                {
                    return 0;
                }

                var state = other as TestGameState;
                if (object.ReferenceEquals(state, null))
                {
                    return 1;
                }

                return this.id.CompareTo(state.id);
            }

            public IReadOnlyList<Move> GetAvailableMoves()
            {
                return this.players.Select(p => new Move(p)).ToList().AsReadOnly();
            }

            public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
            {
                yield return Weighted.Create(this.MakeMove(move), 1);
            }

            public IEnumerable<IGameState<Move>> GetView(PlayerToken playerToken, int maxStates)
            {
                yield return this;
            }

            public IReadOnlyCollection<PlayerToken> GetWinners()
            {
                return new List<PlayerToken>().AsReadOnly();
            }

            public IGameState<Move> MakeMove(Move move)
            {
                return this;
            }

            public class Move : IMove
            {
                public Move(PlayerToken playerToken)
                {
                    this.PlayerToken = playerToken;
                }

                public IList<object> FormatTokens => new object[] { "Test Move" };

                public bool IsDeterministic => true;

                public PlayerToken PlayerToken { get; }
            }
        }
    }
}
