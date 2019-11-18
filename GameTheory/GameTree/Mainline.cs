// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.GameTree
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Records a possible line of gameplay and it's computed scores.
    /// </summary>
    /// <typeparam name="TGameState">The type of game states in the mainline.</typeparam>
    /// <typeparam name="TMove">The type of moves in the mainline.</typeparam>
    /// <typeparam name="TScore">The type used to keep track of score.</typeparam>
    public class Mainline<TGameState, TMove, TScore> : ITokenFormattable
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mainline{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="scores">The computed scores for all players in the resulting state.</param>
        /// <param name="state">The resulting game state.</param>
        /// <param name="strategies">The sequence of moves necessary to arrive at the resulting game state.</param>
        /// <param name="playerToken">The player who moves next in the sequence or <c>null</c> if there are no moves.</param>
        /// <param name="depth">The depth to which the score was computed.</param>
        /// <param name="fullyDetermined">A flag indicating whether or not the game tree has been exhaustively searched at this node.</param>
        public Mainline(IDictionary<PlayerToken, TScore> scores, TGameState state, PlayerToken playerToken, ImmutableStack<IReadOnlyList<IWeighted<TMove>>> strategies, int depth, bool fullyDetermined)
        {
            this.Scores = scores;
            this.GameState = state;
            this.PlayerToken = playerToken;
            this.Strategies = strategies;
            this.Depth = depth;
            this.FullyDetermined = fullyDetermined;
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
                foreach (var strategy in this.Strategies)
                {
                    if (!first)
                    {
                        tokens.Add("; ");
                    }

                    first = false;

                    var totalWeight = 0.0;
                    if (strategy.Count > 1)
                    {
                        tokens.Add("{");
                        totalWeight = strategy.Sum(m => m.Weight);
                    }

                    var firstMove = true;
                    foreach (var move in strategy)
                    {
                        if (!firstMove)
                        {
                            tokens.Add(", ");
                        }

                        firstMove = false;

                        tokens.Add(move.Value);

                        if (strategy.Count > 1)
                        {
                            tokens.Add(" @");
                            tokens.Add((move.Weight / totalWeight).ToString("P0"));
                        }
                    }

                    if (strategy.Count > 1)
                    {
                        tokens.Add("}");
                    }
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

                tokens.Add("] (depth ");

                if (!this.FullyDetermined)
                {
                    tokens.Add(">=");
                }

                tokens.Add(this.Depth);
                tokens.Add(")");

                return tokens;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the entire game tree has been searched at this level.
        /// </summary>
        public bool FullyDetermined { get; }

        /// <summary>
        /// Gets the resulting game state.
        /// </summary>
        public TGameState GameState { get; }

        /// <summary>
        /// Gets the player who moves next in the sequence or <c>null</c> if there are no moves.
        /// </summary>
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets the computed scores for all players in the resulting state.
        /// </summary>
        public IDictionary<PlayerToken, TScore> Scores { get; }

        /// <summary>
        /// Gets the sequence of moves/strategies necessary to arrive at the resulting game state.
        /// </summary>
        public ImmutableStack<IReadOnlyList<IWeighted<TMove>>> Strategies { get; }

        /// <summary>
        /// Extends the mainline by one ply specifying the strategy to play, optionally extending the score by one ply as well.
        /// </summary>
        /// <param name="player">The player executing the <paramref name="strategy"/>.</param>
        /// <param name="strategy">The strategy to execute at this stage in the game tree.</param>
        /// <param name="scoreExtender">The optional <see cref="IScorePlyExtender{TScore}"/> to use to extend the score.</param>
        /// <returns>The extended mainline.</returns>
        public Mainline<TGameState, TMove, TScore> Extend(PlayerToken player, ImmutableArray<IWeighted<TMove>> strategy, IScorePlyExtender<TScore> scoreExtender = null)
        {
            var scores = this.Scores;

            if (scoreExtender != null)
            {
                var newScores = new Dictionary<PlayerToken, TScore>(scores.Count);

                foreach (var p in this.GameState.Players)
                {
                    newScores[p] = scoreExtender.Extend(scores[p]);
                }

                scores = newScores;
            }

            return new Mainline<TGameState, TMove, TScore>(scores, this.GameState, player, this.Strategies.Push(strategy), this.Depth + 1, this.FullyDetermined);
        }

        /// <inheritdoc/>
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
