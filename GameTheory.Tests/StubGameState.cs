﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    internal class StubGameState : IGameState<StubGameState.Move>
    {
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

        public IReadOnlyList<PlayerToken> Players
        {
            get { return this.players; }
        }

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

        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken playerToken)
        {
            return this.moves.Where(m => m.PlayerToken == playerToken).ToList().AsReadOnly();
        }

        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            return this.winnersReadOnly.Distinct().ToList().AsReadOnly();
        }

        public IGameState<Move> MakeMove(Move move)
        {
            return this;
        }

        public class Move : IMove
        {
            private readonly PlayerToken playerToken;
            private readonly string value;

            public Move(PlayerToken playerToken, string value)
            {
                this.playerToken = playerToken;
                this.value = value;
            }

            public PlayerToken PlayerToken
            {
                get { return this.playerToken; }
            }

            public string Value
            {
                get { return this.value; }
            }
        }
    }
}
