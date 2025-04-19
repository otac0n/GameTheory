// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move in Love Letter.
    /// </summary>
    public abstract class Move : IMove, IComparable<Move>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="LoveLetter.GameState"/> that this move is based on.</param>
        public Move(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            this.GameState = state;
            this.PlayerToken = state.ActivePlayer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="state">The <see cref="LoveLetter.GameState"/> that this move is based on.</param>
        /// <param name="player">The <see cref="PlayerToken">player</see> that may choose this move.</param>
        protected Move(GameState state, PlayerToken player)
        {
            ArgumentNullException.ThrowIfNull(state);

            this.GameState = state;
            this.PlayerToken = player;
        }

        /// <inheritdoc />
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc />
        public virtual bool IsDeterministic => true;

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
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());

        internal virtual GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            if (state.Phase == Phase.Draw)
            {
                state = state.With(
                    phase: Phase.Discard);
            }
            else if (state.Phase == Phase.Discard)
            {
                var nextPlayer = activePlayer;
                if (state.Deck.Count > 0 && state.Inventory.Values.Where(inv => inv.Hand.Length > 0).Take(2).Count() > 1)
                {
                    do
                    {
                        nextPlayer = state.GetNextPlayer<GameState, Move>(nextPlayer);
                    }
                    while (nextPlayer != activePlayer && state.Inventory[nextPlayer].Hand.Length == 0);
                }

                if (nextPlayer == activePlayer)
                {
                    state = state.With(
                        phase: Phase.Reveal);
                }
                else
                {
                    var inventory = state.Inventory;
                    foreach (var player in inventory.Keys)
                    {
                        if (inventory[player].HandRevealed)
                        {
                            inventory = inventory.SetItem(
                                player,
                                inventory[player].With(handRevealed: false));
                        }
                    }

                    state = state.With(
                        activePlayer: nextPlayer,
                        inventory: inventory,
                        phase: Phase.Draw);
                }
            }
            else if (state.Phase == Phase.Reveal)
            {
                if (!state.Inventory.Values.Any(inv => !inv.HandRevealed && inv.Hand.Length > 0))
                {
                    var inventory = state.Inventory;
                    var winner = inventory
                        .Where(s => s.Value.Hand.Length > 0)
                        .OrderByDescending(s => s.Value.Hand.Max(c => (int)c))
                        .ThenByDescending(s => s.Value.Discards.Sum(c => (int)c))
                        .Select(s => s.Key)
                        .First();

                    var winnerInventory = inventory[winner];
                    winnerInventory = winnerInventory.With(
                        tokens: winnerInventory.Tokens + 1);

                    inventory = inventory.SetItem(
                        winner,
                        winnerInventory);

                    if (winnerInventory.Tokens >= GameState.WinThresholds[state.Players.Length])
                    {
                        state = state.With(
                            inventory: inventory,
                            phase: Phase.End);
                    }
                    else
                    {
                        state = state.With(
                            activePlayer: winner,
                            inventory: inventory,
                            phase: Phase.Deal);
                    }
                }
            }
            else if (state.Phase == Phase.Deal)
            {
                state = state.With(
                    phase: Phase.Draw);
            }

            return state;
        }

        internal virtual IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state) => throw new NotImplementedException();
    }
}
