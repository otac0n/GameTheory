﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class StubGameState : IGameState<StubGameState.Move>
    {
        private readonly Guid id = Guid.NewGuid();
        private readonly List<Move> moves;
        private readonly IReadOnlyList<Move> movesReadOnly;
        private readonly IReadOnlyList<PlayerToken> players;
        private readonly List<PlayerToken> winners;
        private readonly IReadOnlyList<PlayerToken> winnersReadOnly;

        public StubGameState(int playerCount = 1)
        {
            this.movesReadOnly = (this.moves = new List<Move>()).AsReadOnly();
            this.winnersReadOnly = (this.winners = new List<PlayerToken>()).AsReadOnly();
            this.players = Enumerable.Range(0, playerCount).Select(i => new PlayerToken()).ToList().AsReadOnly();
        }

        public IReadOnlyList<Move> Moves
        {
            get
            {
                return this.movesReadOnly;
            }

            set
            {
                this.moves.Clear();
                if (value != null)
                {
                    this.moves.AddRange(value);
                }
            }
        }

        public IReadOnlyList<PlayerToken> Players => this.players;

        public IReadOnlyList<PlayerToken> Winners
        {
            get
            {
                return this.winnersReadOnly;
            }

            set
            {
                this.winners.Clear();
                if (value != null)
                {
                    this.winners.AddRange(value);
                }
            }
        }

        public IReadOnlyList<Move> GetAvailableMoves()
        {
            return this.moves.AsReadOnly();
        }

        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            return this.winnersReadOnly.Distinct().ToList().AsReadOnly();
        }

        public IGameState<Move> MakeMove(Move move)
        {
            return this;
        }

        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        public IGameState<Move> GetView(PlayerToken playerToken)
        {
            return this;
        }

        public int CompareTo(IGameState<Move> other)
        {
            if (other == this)
            {
                return 0;
            }

            var state = other as StubGameState;
            if (state == null)
            {
                return 1;
            }

            return this.id.CompareTo(state.id);
        }

        public class Move : IMove
        {
            public Move(PlayerToken playerToken, string value)
            {
                this.PlayerToken = playerToken;
                this.Value = value;
            }

            public PlayerToken PlayerToken { get; }

            public bool IsDeterministic => true;

            public string Value { get; }
        }
    }
}
