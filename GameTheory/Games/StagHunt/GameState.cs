// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.StagHunt
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the current state in a game of Stag Hunt.
    /// </summary>
    public sealed class GameState : NormalFormGame.GameState<string>
    {
        /// <summary>
        /// A move of "hare".
        /// </summary>
        public const string Hare = "Hare";

        /// <summary>
        /// A move of "stag".
        /// </summary>
        public const string Stag = "Stag";

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
                ? this.Choices[p] == Stag ? 3 : 1
                : this.Choices[p] == Hare ? 2 : 0;
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetMoveKinds(PlayerToken playerToken)
        {
            return new[] { Stag, Hare };
        }

        /// <inheritdoc />
        protected override NormalFormGame.GameState<string> WithChoices(ImmutableArray<string> choices)
        {
            return new GameState(this.Players, choices);
        }
    }
}
