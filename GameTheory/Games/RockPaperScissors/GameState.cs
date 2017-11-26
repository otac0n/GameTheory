// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.RockPaperScissors
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Implements the game of Stag Hunt.
    /// </summary>
    public class GameState : NormalFormGame.GameState<string>
    {
        /// <summary>
        /// A move of "paper".
        /// </summary>
        public const string Paper = "Paper";

        /// <summary>
        /// A move of "rock".
        /// </summary>
        public const string Rock = "Rock";

        /// <summary>
        /// A move of "scissors".
        /// </summary>
        public const string Scissors = "Scissors";

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        public GameState()
            : base()
        {
        }

        private GameState(ImmutableArray<PlayerToken> players, ImmutableArray<string> choices)
            : base(players, choices)
        {
        }

        /// <inheritdoc />
        public override double GetScore(PlayerToken player)
        {
            if (this.Choices.Any(c => c == null))
            {
                return 0;
            }

            var p = this.Players.IndexOf(player);
            var o = 1 - p;

            if ((this.Choices[p] == Rock && this.Choices[o] == Scissors) ||
                (this.Choices[p] == Paper && this.Choices[o] == Rock) ||
                (this.Choices[p] == Scissors && this.Choices[o] == Paper))
            {
                return 1;
            }

            return 0;
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetMoveKinds(PlayerToken playerToken)
        {
            return new[] { Rock, Paper, Scissors };
        }

        /// <inheritdoc />
        protected override NormalFormGame.GameState<string> WithChoices(ImmutableArray<string> choices)
        {
            return new GameState(this.Players, choices);
        }
    }
}
