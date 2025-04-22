// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move in <see cref="Ergo.GameState">Ergo</see>.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="Ergo.GameState"/> that this move is based on.</param>
        internal Move(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            this.GameState = state;
            this.PlayerToken = state.ActivePlayer;
        }

        /// <inheritdoc/>
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc/>
        public virtual bool IsDeterministic => true;

        /// <inheritdoc/>
        public PlayerToken PlayerToken { get; }

        internal GameState GameState { get; }

        /// <inheritdoc />
        public virtual int CompareTo(Move other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            return string.Compare(this.GetType().Name, other.GetType().Name, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            if (state.Phase == Phase.Draw)
            {
                state = state.With(
                    phase: Phase.Play);
            }
            else if (state.Phase == Phase.Deal)
            {
                state = state.With(
                    phase: Phase.Draw,
                    activePlayer: state.Players.GetNextPlayer(state.Dealer));
            }
            else if (state.Phase == Phase.Play)
            {
                var remainingActions = state.RemainingActions - 1;
                var activePlayer = state.ActivePlayer;
                var dealer = state.Dealer;
                var phase = state.Phase;
                var fallacyCounter = state.FallacyCounter;
                var scores = state.Scores;

                if (remainingActions == 0 || state.IsRoundOver)
                {
                    remainingActions = GameState.ActionsPerTurn;

                    if (state.Deck.Count == 0 || state.IsRoundOver)
                    {
                        IList<PlayerToken> winningPlayers;
                        if (state.IsProofValid && Compiler.TryCompileProof(state.Proof, out var compiled))
                        {
                            var compiledFunc = compiled.Compile();
                            var satisfied = Enumerable.Range(0, 1 << GameState.MaxPlayers)
                                .Select(x => new[] { (x & 0b0001) == 0, (x & 0b0010) == 0, (x & 0b0100) == 0, (x & 0b1000) == 0 })
                                .Where(x => compiledFunc(x[0], x[1], x[2], x[2]))
                                .ToList();

                            winningPlayers = satisfied.Count == 0
                                ? (IList<PlayerToken>)ImmutableList<PlayerToken>.Empty
                                : Enumerable.Range(0, state.Players.Length).Where(i => satisfied.All(x => x[i])).Select(i => state.Players[i]).ToList();
                        }
                        else
                        {
                            winningPlayers = ImmutableList<PlayerToken>.Empty;
                        }

                        if (winningPlayers.Count > 0)
                        {
                            var cardCount = state.Proof.Sum(p => p.Count);
                            foreach (var player in winningPlayers)
                            {
                                scores = scores.SetItem(player, scores[player] + cardCount);
                            }
                        }

                        if (scores.Values.Any(v => v >= GameState.TargetPoints))
                        {
                            phase = Phase.End;
                        }
                        else
                        {
                            phase = Phase.Deal;
                            activePlayer = dealer = state.Players.GetNextPlayer(dealer);
                        }
                    }
                    else
                    {
                        var fallacyCount = fallacyCounter[activePlayer];
                        if (fallacyCount > 0)
                        {
                            fallacyCounter = fallacyCounter.SetItem(activePlayer, fallacyCount - 1);
                        }

                        phase = Phase.Draw;
                        activePlayer = state.Players.GetNextPlayer(activePlayer);
                    }
                }

                state = state.With(
                    phase: phase,
                    remainingActions: remainingActions,
                    activePlayer: activePlayer,
                    dealer: dealer,
                    scores: scores,
                    fallacyCounter: fallacyCounter);
            }

            return state;
        }

        internal virtual IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state) => throw new NotImplementedException();
    }
}
