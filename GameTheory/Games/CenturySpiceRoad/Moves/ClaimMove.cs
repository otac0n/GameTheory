﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class ClaimMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="index">The index of the card to claim.</param>
        public ClaimMove(GameState state, int index)
            : base(state)
        {
            this.Index = index;
        }

        /// <inheritdoc />
        public override bool IsDeterministic => this.State.PointCardDeck.Count <= 1;

        /// <summary>
        /// Gets the index of the card to claim.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override string ToString() => $"Claim {this.State.PointCardTrack[this.Index]}";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var inventory = state.Inventory[activePlayer];
            for (var i = 0; i < state.PointCardTrack.Count; i++)
            {
                var card = state.PointCardTrack[i];
                if (card.Cost.Keys.All(k => inventory.Caravan[k] >= card.Cost[k]))
                {
                    yield return new ClaimMove(state, i);
                }
            }
        }

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];
            var pTokens = pInventory.Tokens;
            var claimed = state.PointCardTrack[this.Index];
            var tokens = state.Tokens;

            var availableTokens = tokens.Keys.ToList();
            if (this.Index < availableTokens.Count)
            {
                tokens = tokens.Remove(availableTokens[this.Index]);
                pTokens = pTokens.Add(availableTokens[this.Index]);
            }

            pInventory = pInventory.With(
                pointCards: pInventory.PointCards.Add(claimed),
                tokens: pTokens,
                caravan: pInventory.Caravan.RemoveRange(claimed.Cost));

            var pointCardDeck = state.PointCardDeck.Deal(1, out ImmutableList<PointCard> dealt);
            var pointCardTrack = state.PointCardTrack
                .RemoveAt(this.Index)
                .AddRange(dealt);

            state = state.With(
                pointCardDeck: pointCardDeck,
                pointCardTrack: pointCardTrack,
                tokens: tokens,
                inventory: state.Inventory.SetItem(activePlayer, pInventory));

            return base.Apply(state);
        }
    }
}