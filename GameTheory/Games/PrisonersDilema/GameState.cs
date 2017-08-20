// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PrisonersDilema
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Implements the game of Prisoner's Dilema.
    /// </summary>
    public class GameState : NormalFormGame.GameState<string>
    {
        /// <summary>
        /// A move of "cooperate".
        /// </summary>
        public const string Cooperate = "Cooperate";

        /// <summary>
        /// A move of "defect".
        /// </summary>
        public const string Defect = "Defect";

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

            var match = this.Choices[0] == this.Choices[1];
            var p = this.Players.IndexOf(player);
            return match
                ? this.Choices[p] == Cooperate ? -1 : -2
                : this.Choices[p] == Cooperate ? -5 : 0;
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetMoveKinds()
        {
            return new[] { Cooperate, Defect };
        }

        /// <inheritdoc />
        protected override NormalFormGame.GameState<string> WithChoices(ImmutableArray<string> choices)
        {
            return new GameState(this.Players, choices);
        }
    }
}
