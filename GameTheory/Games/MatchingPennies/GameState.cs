// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.MatchingPennies
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Implements the game of matching pennies.
    /// </summary>
    public class GameState : IGameState<Move>
    {
        private readonly ImmutableArray<PlayerToken> players;
        private readonly ImmutableArray<bool?> choices;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class in the starting position.
        /// </summary>
        public GameState()
            : this(ImmutableArray.Create(new PlayerToken(), new PlayerToken()), ImmutableArray.Create<bool?>(null, null))
        {
        }

        private GameState(ImmutableArray<PlayerToken> players, ImmutableArray<bool?> choices)
        {
            this.players = players;
            this.choices = choices;
        }

        /// <inheritdoc />
        public IReadOnlyList<PlayerToken> Players => this.players;

        /// <inheritdoc />
        public IReadOnlyCollection<Move> GetAvailableMoves()
        {
            return this.players
                .Where(p => this.choices[this.players.IndexOf(p)] == null)
                .SelectMany(p => new[] { new Move(p, true), new Move(p, false) })
                .ToImmutableArray();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.choices.Any(c => c == null))
            {
                return ImmutableArray<PlayerToken>.Empty;
            }

            return ImmutableArray.Create(this.players[this.choices.Distinct().Count() - 1]);
        }

        /// <inheritdoc />
        IGameState<Move> IGameState<Move>.MakeMove(Move move) => this.MakeMove(move);

        /// <summary>
        /// Applies the move to the current game state.
        /// </summary>
        /// <param name="move">The <see cref="Move"/> to apply.</param>
        /// <returns>The updated <see cref="GameState"/>.</returns>
        public GameState MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            var index = this.players.IndexOf(move.PlayerToken);

            return new GameState(
                players: this.players,
                choices: this.choices.SetItem(index, move.Heads));
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move>>> GetOutcomes(Move move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <inheritdoc />
        public IGameState<Move> GetView(PlayerToken playerToken)
        {
            var index = this.players.IndexOf(playerToken);
            if (index == -1)
            {
                throw new InvalidOperationException();
            }

            return new GameState(
                players: this.players,
                choices: this.choices.SetItem(1 - index, null));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string Penny(bool? choice)
            {
                switch (choice)
                {
                    case true:
                        return "H";
                    case false:
                        return "T";
                    default:
                        return "?";
                }
            }

            return $"[{Penny(this.choices[0])}{Penny(this.choices[1])}]";
        }
    }
}
