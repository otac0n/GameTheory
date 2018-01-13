// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Records a possible line of gameplay and it's computed scores.
    /// </summary>
    /// <typeparam name="TMove">The type of moves being recorded.</typeparam>
    /// <typeparam name="TScore">The score being assigned to the state.</typeparam>
    public class Mainline<TMove, TScore> : ITokenFormattable
        where TMove : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mainline{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="scores">The computed scores for all players in the resulting state.</param>
        /// <param name="gameState">The resulting game state.</param>
        /// <param name="moves">The sequence of moves necessary to arrive at the resulting game state.</param>
        /// <param name="depth">The depth to which the score was computed.</param>
        public Mainline(IReadOnlyDictionary<PlayerToken, TScore> scores, IGameState<TMove> gameState, ImmutableStack<TMove> moves, int depth)
        {
            this.Scores = scores;
            this.GameState = gameState;
            this.Moves = moves;
            this.Depth = depth;
        }

        /// <summary>
        /// Gets the depth to which the score was computed.
        /// </summary>
        public int Depth { get; }

        /// <inheritdoc/>
        public IList<object> FormatTokens
        {
            get
            {
                var tokens = new List<object>();

                var first = true;
                foreach (var move in this.Moves)
                {
                    if (!first)
                    {
                        tokens.Add(" ");
                    }

                    tokens.Add(move);
                    first = false;
                }

                tokens.Add(" [");

                first = true;
                foreach (var score in this.Scores)
                {
                    if (!first)
                    {
                        tokens.Add(", ");
                    }

                    tokens.Add(score.Key);
                    tokens.Add(": ");
                    tokens.Add(score.Value);

                    first = false;
                }

                tokens.Add("]");

                return tokens;
            }
        }

        /// <summary>
        /// Gets the resulting game state.
        /// </summary>
        public IGameState<TMove> GameState { get; }

        /// <summary>
        /// Gets the sequence of moves necessary to arrive at the resulting game state.
        /// </summary>
        public ImmutableStack<TMove> Moves { get; }

        /// <summary>
        /// Gets the computed scores for all players in the resulting state.
        /// </summary>
        public IReadOnlyDictionary<PlayerToken, TScore> Scores { get; }

        /// <summary>
        /// Returns a new game state with the specified move prepended.
        /// </summary>
        /// <param name="move">The move necessary to arrive at this mainline.</param>
        /// <returns>The new mainline.</returns>
        public Mainline<TMove, TScore> AddMove(TMove move)
        {
            return new Mainline<TMove, TScore>(
                this.Scores,
                this.GameState,
                this.Moves.Push(move),
                this.Depth + 1);
        }

        /// <inheritdoc/>
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
