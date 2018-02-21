// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.NormalFormGame.Chicken
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Matching Pennies.
    /// </summary>
    public sealed class GameState : GameState<string>
    {
        /// <summary>
        /// A move of "Keep Going".
        /// </summary>
        public const string KeepGoing = nameof(KeepGoing);

        /// <summary>
        /// A move of "Swerve".
        /// </summary>
        public const string Swerve = nameof(Swerve);

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

            return this.Choices[p] == KeepGoing && this.Choices[o] == KeepGoing ? -100 :
                   this.Choices[p] == KeepGoing ? 2 :
                   this.Choices[o] == KeepGoing ? -2 :
                   0;
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetMoveKinds(PlayerToken playerToken)
        {
            return new[] { Swerve, KeepGoing };
        }

        /// <inheritdoc />
        protected override GameState<string> WithChoices(ImmutableArray<string> choices)
        {
            return new GameState(this.Players, choices);
        }
    }
}
