// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Players.MaximizingPlayer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GameTheory.GameTree;

    public abstract class MaximizingPlayerBase<TMove, TScore> : IPlayer<TMove>
        where TMove : IMove
    {
        protected readonly GameTree<TMove, TScore> gameTree;
        protected readonly IScorePlyExtender<TScore> scoreExtender;
        protected readonly IGameStateScoringMetric<TMove, TScore> scoringMetric;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximizingPlayerBase{TMove, TScore}"/> class.
        /// </summary>
        /// <param name="playerToken">The token that represents the player.</param>
        /// <param name="scoringMetric">The scoring metric to use.</param>
        public MaximizingPlayerBase(PlayerToken playerToken, IGameStateScoringMetric<TMove, TScore> scoringMetric)
        {
            this.scoringMetric = scoringMetric ?? throw new ArgumentNullException(nameof(scoringMetric));
            this.scoreExtender = this.scoringMetric as IScorePlyExtender<TScore>;
            this.PlayerToken = playerToken;
            this.gameTree = new GameTree<TMove, TScore>(this.MakeCache());
        }

        /// <inheritdoc />
        public event EventHandler<MessageSentEventArgs> MessageSent;

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets the maximum number of states to sample from all possible initial states.
        /// </summary>
        protected virtual int InitialSamples => 30;

        /// <summary>
        /// Gets a value indicating whether or not the player will calculate during an opponent's turn.
        /// </summary>
        protected virtual bool Ponder => true;

        /// <inheritdoc />
        public async Task<Maybe<TMove>> ChooseMove(IGameState<TMove> state, CancellationToken cancel)
        {
            await Task.Yield();

            var sw = Stopwatch.StartNew();
            var moves = state.GetAvailableMoves();

            var players = moves.Select(m => m.PlayerToken).ToImmutableHashSet();
            var ponder = false;
            if (!players.Contains(this.PlayerToken))
            {
                if (!this.Ponder || moves.Count == 0)
                {
                    return default(Maybe<TMove>);
                }
                else
                {
                    ponder = true;
                }
            }
            else
            {
                if (players.Count == 1 && moves.Count == 1)
                {
                    // If we are forced to move, don't spend any time determining the correct move.
                    return moves.Single();
                }
            }

            var states = state.GetView(this.PlayerToken, this.InitialSamples).ToList();

            var mainline = this.GetMove(states, ponder, cancel);

            if (mainline == null || mainline.PlayerToken != this.PlayerToken)
            {
                return default(Maybe<TMove>);
            }
            else
            {
                var chosen = mainline.Strategies.Peek().Pick();
                sw.Stop();
                this.gameTree.Trim();
                return chosen;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected Mainline<TMove, TScore> CombineMainlines(IList<Mainline<TMove, TScore>> mainlines)
        {
            if (mainlines.Count == 1)
            {
                return mainlines[0];
            }

            var moveWeights = new Dictionary<TMove, Weighted<Mainline<TMove, TScore>>>(new ComparableEqualityComparer<TMove>());
            var fullyDetermined = true;

            // Single Monte-Carlo
            foreach (var mainline in mainlines)
            {
                fullyDetermined &= mainline.FullyDetermined;

                if (!mainline.Strategies.IsEmpty)
                {
                    var strategy = mainline.Strategies.Peek();
                    foreach (var move in strategy)
                    {
                        if (moveWeights.TryGetValue(move.Value, out var weighted))
                        {
                            weighted = Weighted.Create(weighted.Value, weighted.Weight + move.Weight);
                        }
                        else
                        {
                            weighted = Weighted.Create(mainline, move.Weight);
                        }

                        moveWeights[move.Value] = weighted;
                    }
                }
            }

            var sourceMainline = mainlines.Pick();
            var maxMoves = moveWeights.Select(m => (IWeighted<TMove>)Weighted.Create(m.Key, m.Value.Weight)).ToImmutableArray();
            var sourceStrategy = sourceMainline.Strategies.IsEmpty
                ? maxMoves.Length > 0 ? sourceMainline.Strategies.Push(maxMoves) : sourceMainline.Strategies
                : sourceMainline.Strategies.Pop().Push(maxMoves);
            var depth = fullyDetermined ? mainlines.Max(m => m.Depth) : mainlines.Where(m => !m.FullyDetermined).Min(m => m.Depth);
            var scores = (from m in mainlines
                          from s in m.Scores
                          group s.Value by s.Key into g
                          select new
                          {
                              PlayerToken = g.Key,
                              Score = this.scoringMetric.Combine(g.Select(s => Weighted.Create(s, 1)).ToArray()),
                          }).ToDictionary(m => m.PlayerToken, m => m.Score);
            return new Mainline<TMove, TScore>(scores, sourceMainline.GameState, sourceMainline.PlayerToken, sourceStrategy, depth, fullyDetermined);
        }

        protected Mainline<TMove, TScore> CombineOutcomes(IGameState<TMove> state, IList<Weighted<Mainline<TMove, TScore>>> mainlines)
        {
            var firstMainline = mainlines[0].Value;
            if (mainlines.Count == 1)
            {
                return firstMainline;
            }

            var maxWeight = double.NegativeInfinity;
            Mainline<TMove, TScore> maxMainline = null;
            var fullyDetermined = true;
            var unvisited = 0;
            for (var i = 0; i < mainlines.Count; i++)
            {
                var weightedMainline = mainlines[i];
                var weight = weightedMainline.Weight;
                var mainline = weightedMainline.Value;
                if (mainline == null)
                {
                    unvisited++;
                    fullyDetermined = false;
                }
                else
                {
                    var score = mainline.Scores;
                    var playerToken = mainline.PlayerToken;

                    var weightCompare = weight.CompareTo(maxWeight);
                    if (weightCompare > 0)
                    {
                        maxWeight = weight;
                        maxMainline = mainline;
                    }

                    fullyDetermined &= mainline.FullyDetermined;
                }
            }

            var combinedScore = new Dictionary<PlayerToken, TScore>();
            var playerScore = new Weighted<TScore>[mainlines.Count - unvisited];
            foreach (var player in state.Players)
            {
                var j = 0;
                for (var i = 0; i < mainlines.Count; i++)
                {
                    if (mainlines[i].Value != null)
                    {
                        playerScore[j++] = Weighted.Create(mainlines[i].Value.Scores[player], mainlines[i].Weight);
                    }
                }

                combinedScore[player] = this.scoringMetric.Combine(playerScore);
            }

            var depth = fullyDetermined ? mainlines.Max(m => m.Value.Depth) : mainlines.Where(m => !m.Value.FullyDetermined).Min(m => m.Value.Depth);
            return new Mainline<TMove, TScore>(combinedScore, maxMainline.GameState, maxMainline.PlayerToken, maxMainline.Strategies, depth, fullyDetermined);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Get the lead of a specific player over the field.
        /// </summary>
        /// <param name="mainline">The mainline contianing the state and score.</param>
        /// <param name="player">The player whose lead should be retrieved.</param>
        /// <returns>The player's lead, as a score.</returns>
        protected TScore GetLead(Mainline<TMove, TScore> mainline, PlayerToken player) => this.GetLead(mainline.Scores, mainline.GameState, player);

        /// <summary>
        /// Gets the lead of a specific player over the field.  In order to support games of 3 or more players, this class tries to maximize the lead rather than the actual score value.
        /// In cooperative games, override this to ensure that coalitions of players cooperate.
        /// </summary>
        /// <param name="score">Contains the scores for the players in the specified game state.</param>
        /// <param name="state">The <see cref="IGameState{TMove}"/> that was scored.</param>
        /// <param name="player">The player whose lead should be retrieved.</param>
        /// <returns>The player's lead, as a score.</returns>
        protected virtual TScore GetLead(IDictionary<PlayerToken, TScore> score, IGameState<TMove> state, PlayerToken player)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }

            if (state.Players.Count == 1)
            {
                return score[player];
            }
            else
            {
                var any = false;
                var max = default(TScore);
                var players = state.Players;
                var count = players.Count;
                for (var i = 0; i < count; i++)
                {
                    var other = players[i];
                    if (other != player)
                    {
                        var value = score[other];
                        if (!any || this.scoringMetric.Compare(value, max) > 0)
                        {
                            max = value;
                            any = true;
                        }
                    }
                }

                return this.scoringMetric.Difference(
                    score[player],
                    max);
            }
        }

        protected abstract Mainline<TMove, TScore> GetMove(List<IGameState<TMove>> states, bool ponder, CancellationToken cancel);

        /// <summary>
        /// Create the transposition table cache.
        /// </summary>
        /// <returns>The cache object to use as a transposition table.</returns>
        protected abstract IGameStateCache<TMove, TScore> MakeCache();

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="formatTokens">The message to send.</param>
        protected void SendMessage(params object[] formatTokens)
        {
            this.MessageSent?.Invoke(this, new MessageSentEventArgs(formatTokens));
        }
    }
}
