// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to claim a point card.
    /// </summary>
    public sealed class ClaimMove : Move
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
        public override IList<object> FormatTokens => new object[] { "Claim ", this.State.PointCardTrack[this.Index] };

        /// <summary>
        /// Gets the index of the card to claim.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public override bool IsDeterministic => this.State.PointCardDeck.Count <= 1;

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
            var inventory = state.Inventory.SetItem(activePlayer, pInventory);
            var pointCardTrack = state.PointCardTrack.RemoveAt(this.Index);

            var pointCardDeck = state.PointCardDeck.Deal(1, out ImmutableList<PointCard> dealt);
            pointCardTrack = pointCardTrack.AddRange(dealt);

            state = state.With(
                pointCardDeck: pointCardDeck,
                pointCardTrack: pointCardTrack,
                tokens: tokens,
                inventory: inventory);

            return base.Apply(state);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
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

            var inventory = state.Inventory.SetItem(activePlayer, pInventory);
            var pointCardTrack = state.PointCardTrack.RemoveAt(this.Index);

            if (state.PointCardDeck.Count == 0)
            {
                var outcome = base.Apply(state.With(
                    pointCardTrack: pointCardTrack,
                    tokens: tokens,
                    inventory: inventory));
                yield return Weighted.Create(outcome, 1);
            }
            else
            {
                for (var i = 0; i < state.PointCardDeck.Count; i++)
                {
                    var outcome = base.Apply(state.With(
                        pointCardDeck: state.PointCardDeck.RemoveAt(i),
                        pointCardTrack: pointCardTrack.Add(state.PointCardDeck[i]),
                        tokens: tokens,
                        inventory: inventory));
                    yield return Weighted.Create(outcome, 1);
                }
            }
        }
    }
}
