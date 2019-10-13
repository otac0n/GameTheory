// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Nessos.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="Nessos.GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        protected Move(GameState state, PlayerToken player)
        {
            this.GameState = state ?? throw new ArgumentNullException(nameof(state));
            this.PlayerToken = player;
        }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public bool IsDeterministic => true;

        /// <inheritdoc />
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
        public override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            var activePlayer = state.FirstPlayer;

            if (state.Phase == Phase.Draw)
            {
                var pointThreshold = GameState.PointThresholds[state.Players.Length];
                var endPhase = false;
                var charons = 0;
                var remainingPlayers = 0;
                foreach (var player in state.Players)
                {
                    var inventory = state.Inventory[player];

                    var playerCharons = inventory.OwnedCards[Card.Charon];
                    charons += playerCharons;
                    if (playerCharons < GameState.PlayerCharonLimit)
                    {
                        remainingPlayers++;

                        if (GameState.Score(inventory.OwnedCards) >= pointThreshold)
                        {
                            endPhase = true;
                            break;
                        }
                    }

                    if (charons >= GameState.TotalCharonLimit)
                    {
                        endPhase = true;
                        break;
                    }
                }

                if (endPhase || remainingPlayers <= 1)
                {
                    state = state.With(
                        phase: Phase.End);
                }
                else
                {
                    if (state.Deck.Count == 0 || state.Inventory.All(i => i.Value.OwnedCards[Card.Charon] >= GameState.PlayerCharonLimit || i.Value.Hand.Count >= GameState.HandLimit))
                    {
                        var firstPlayer = state.FirstPlayer;
                        do
                        {
                            firstPlayer = state.GetNextPlayer(firstPlayer);
                        }
                        while (firstPlayer != state.FirstPlayer && state.Inventory[firstPlayer].OwnedCards[Card.Charon] >= GameState.PlayerCharonLimit);

                        state = state.With(
                            firstPlayer: firstPlayer,
                            phase: Phase.Offer);
                    }
                }
            }

            return state;
        }
    }
}
