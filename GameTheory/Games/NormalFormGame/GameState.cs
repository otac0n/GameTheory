// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.NormalFormGame
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Implements the game of matching pennies.
    /// </summary>
    public abstract class GameState<T> : IGameState<Move<T>>
        where T : class, IComparable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameState{T}"/> class in the starting position.
        /// </summary>
        public GameState()
            : this(ImmutableArray.Create(new PlayerToken(), new PlayerToken()), ImmutableArray.Create<T>(null, null))
        {
        }

        protected GameState(ImmutableArray<PlayerToken> players, ImmutableArray<T> choices)
        {
            this.Players = players;
            this.Choices = choices;
        }

        /// <summary>
        /// Gets a list of players in the current game state.
        /// </summary>
        /// <returns>The list of players in the current game state.</returns>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <summary>
        /// Gets the choices that have been made.
        /// </summary>
        public ImmutableArray<T> Choices { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move<T>>.Players => this.Players;

        /// <inheritdoc />
        public IReadOnlyList<Move<T>> GetAvailableMoves()
        {
            return this.Players
                .Where(p => this.Choices[this.Players.IndexOf(p)] == null)
                .SelectMany(p => this.GetMoveKinds().Select(k => new Move<T>(p, k)))
                .ToImmutableArray();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<PlayerToken> GetWinners()
        {
            if (this.Choices.Any(c => c == null))
            {
                return ImmutableArray<PlayerToken>.Empty;
            }

            return this.Players
                .GroupBy(p => this.GetScore(p))
                .OrderByDescending(g => g.Key)
                .First()
                .ToImmutableList();
        }

        /// <summary>
        /// Gets the payoff of the specified player.
        /// </summary>
        /// <param name="player">The player to evaluate.</param>
        /// <returns>The payoff of the specified player.</returns>
        public abstract double GetScore(PlayerToken player);

        /// <inheritdoc />
        IGameState<Move<T>> IGameState<Move<T>>.MakeMove(Move<T> move) => this.MakeMove(move);

        /// <summary>
        /// Applies the move to the current game state.
        /// </summary>
        /// <param name="move">The <see cref="Move{T}"/> to apply.</param>
        /// <returns>The updated <see cref="GameState{T}"/>.</returns>
        public GameState<T> MakeMove(Move<T> move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            var index = this.Players.IndexOf(move.PlayerToken);

            return this.WithChoices(
                this.Choices.SetItem(index, move.Kind));
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move<T>>>> GetOutcomes(Move<T> move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <inheritdoc />
        public IGameState<Move<T>> GetView(PlayerToken playerToken)
        {
            var index = this.Players.IndexOf(playerToken);
            if (index == -1)
            {
                throw new InvalidOperationException();
            }

            return this.WithChoices(
                this.Choices.SetItem(1 - index, null));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string Render(T choice) => choice == null ? "?" : choice.ToString();

            return $"[{Render(this.Choices[0])}, {Render(this.Choices[1])}]";
        }

        /// <inheritdoc/>
        public int CompareTo(IGameState<Move<T>> other)
        {
            if (other == this)
            {
                return 0;
            }

            var state = other as GameState<T>;
            if (state == null)
            {
                return 1;
            }

            int comp;

            if (this.Players != state.Players)
            {
                if ((comp = this.Players.Length.CompareTo(state.Players.Length)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.Players.Length; i++)
                {
                    if ((comp = this.Players[i].CompareTo(state.Players[i])) != 0)
                    {
                        return comp;
                    }
                }
            }

            if (this.Choices != state.Choices)
            {
                if ((comp = this.Choices.Length.CompareTo(state.Choices.Length)) != 0)
                {
                    return comp;
                }

                for (var i = 0; i < this.Players.Length; i++)
                {
                    var choice = this.Choices[i];
                    var otherChoice = state.Choices[i];
                    if (choice != otherChoice)
                    {
                        if (choice == null)
                        {
                            return -1;
                        }
                        else if (otherChoice == null)
                        {
                            return 1;
                        }

                        if ((comp = choice.CompareTo(otherChoice)) != 0)
                        {
                            return comp;
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the kinds of moves defined for this game.
        /// </summary>
        /// <returns>An enumerable collection of the moves defined for this game.</returns>
        protected abstract IEnumerable<T> GetMoveKinds();

        /// <summary>
        /// Copies the current gamestate and overwrites the chosen moves.
        /// </summary>
        /// <param name="choices">The choices to use.</param>
        /// <returns>A new gamestate with the specified choices.</returns>
        protected abstract GameState<T> WithChoices(ImmutableArray<T> choices);
    }
}
