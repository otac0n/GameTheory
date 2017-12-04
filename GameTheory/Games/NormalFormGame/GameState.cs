// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.NormalFormGame
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Implements a normal-form game.
    /// </summary>
    /// <typeparam name="T">The type of choices tracked in this game.</typeparam>
    public abstract class GameState<T> : IGameState<Move<T>>
        where T : class, IComparable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameState{T}"/> class in the starting position.
        /// </summary>
        protected GameState()
            : this(ImmutableArray.Create(new PlayerToken(), new PlayerToken()), ImmutableArray.Create<T>(null, null))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState{T}"/> class with the specified internal state.
        /// This construtor is used for copying the internal state and is intended to be used by the <see cref="WithChoices(ImmutableArray{T})"/> method.
        /// </summary>
        /// <param name="players">The players collection.</param>
        /// <param name="choices">The choices collection.</param>
        protected GameState(ImmutableArray<PlayerToken> players, ImmutableArray<T> choices)
        {
            this.Players = players;
            this.Choices = choices;
        }

        /// <summary>
        /// Gets the choices that have been made.
        /// </summary>
        public ImmutableArray<T> Choices { get; }

        /// <summary>
        /// Gets a list of players in the current game state.
        /// </summary>
        /// <returns>The list of players in the current game state.</returns>
        public ImmutableArray<PlayerToken> Players { get; }

        /// <inheritdoc />
        IReadOnlyList<PlayerToken> IGameState<Move<T>>.Players => this.Players;

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

            if ((comp = CompareUtilities.CompareLists(this.Players, state.Players)) != 0 ||
                (comp = CompareUtilities.CompareLists(this.Choices, state.Choices)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc />
        public IReadOnlyList<Move<T>> GetAvailableMoves()
        {
            return this.Players
                .Where(p => this.Choices[this.Players.IndexOf(p)] == null)
                .SelectMany(p => this.GetMoveKinds(p).Select(k => new Move<T>(p, k)))
                .ToImmutableArray();
        }

        /// <inheritdoc />
        public IEnumerable<IWeighted<IGameState<Move<T>>>> GetOutcomes(Move<T> move)
        {
            yield return Weighted.Create(this.MakeMove(move), 1);
        }

        /// <summary>
        /// Gets the payoff of the specified player.
        /// </summary>
        /// <param name="player">The player to evaluate.</param>
        /// <returns>The payoff of the specified player.</returns>
        public abstract double GetScore(PlayerToken player);

        /// <inheritdoc />
        public IEnumerable<IGameState<Move<T>>> GetView(PlayerToken playerToken, int maxStates)
        {
            var index = this.Players.IndexOf(playerToken);
            if (index == -1)
            {
                throw new InvalidOperationException();
            }

            yield return this.WithChoices(
                this.Choices.SetItem(1 - index, null));
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
        public override string ToString()
        {
            string Render(T choice) => choice == null ? "?" : choice.ToString();

            return $"[{Render(this.Choices[0])}, {Render(this.Choices[1])}]";
        }

        /// <summary>
        /// Gets the kinds of moves defined for this game for the specified player.
        /// </summary>
        /// <param name="playerToken">The player who may play the moves.</param>
        /// <returns>An enumerable collection of the moves defined for this game for the specified player.</returns>
        protected abstract IEnumerable<T> GetMoveKinds(PlayerToken playerToken);

        /// <summary>
        /// Copies the current gamestate and overwrites the chosen moves.
        /// </summary>
        /// <param name="choices">The choices to use.</param>
        /// <returns>A new gamestate with the specified choices.</returns>
        protected abstract GameState<T> WithChoices(ImmutableArray<T> choices);
    }
}
