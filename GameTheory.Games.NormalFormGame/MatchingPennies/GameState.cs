﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.NormalFormGame.MatchingPennies
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
        /// A move of "heads".
        /// </summary>
        public const string Heads = nameof(Heads);

        /// <summary>
        /// A move of "tails".
        /// </summary>
        public const string Tails = nameof(Tails);

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
            return ((player == this.Players[0]) ^ match) ? 0 : 1;
        }

        /// <inheritdoc />
        protected override IEnumerable<string> GetMoveKinds(PlayerToken playerToken)
        {
            return new[] { Heads, Tails };
        }

        /// <inheritdoc />
        protected override GameState<string> WithChoices(ImmutableArray<string> choices)
        {
            return new GameState(this.Players, choices);
        }
    }
}
