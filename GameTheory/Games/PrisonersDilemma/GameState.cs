// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.PrisonersDilemma
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Prisoner's Dilemma.
    /// </summary>
    public sealed class GameState : NormalFormGame.GameState<string>
    {
        /// <summary>
        /// A move of "cooperate".
        /// </summary>
        public const string Cooperate = nameof(Cooperate);

        /// <summary>
        /// A move of "defect".
        /// </summary>
        public const string Defect = nameof(Defect);

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
        protected override IEnumerable<string> GetMoveKinds(PlayerToken playerToken)
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
