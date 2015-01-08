// -----------------------------------------------------------------------
// <copyright file="StubGameState.cs" company="(none)">
//   Copyright © 2014 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    internal class StubGameState : IGameState<StubGameState.Move>
    {
        private readonly List<Move> moves;
        private readonly IReadOnlyList<Move> movesReadOnly;
        private readonly IReadOnlyList<PlayerToken> players;

        public StubGameState(int playerCount = 1)
        {
            this.movesReadOnly = (this.moves = new List<Move>()).AsReadOnly();
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

        public IReadOnlyCollection<Move> GetAvailableMoves(PlayerToken player)
        {
            return this.moves.Where(m => m.Player == player).ToList().AsReadOnly();
        }

        public IGameState<Move> MakeMove(Move move)
        {
            return this;
        }

        public class Move : IMove
        {
            private readonly PlayerToken player;
            private readonly string value;

            public Move(PlayerToken player, string value)
            {
                this.player = player;
                this.value = value;
            }

            public PlayerToken Player
            {
                get { return this.player; }
            }

            public string Value
            {
                get { return this.value; }
            }
        }
    }
}
