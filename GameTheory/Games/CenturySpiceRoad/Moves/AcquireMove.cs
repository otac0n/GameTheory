// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class AcquireMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcquireMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public AcquireMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override bool IsDeterministic => this.State.MerchantCardDeck.Count <= 1;

        /// <inheritdoc />
        public override string ToString() => $"Acquire [{this.State.MerchantCardTrack[this.State.MerchantCardIndexAfforded].MerchantCard}]";

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            if (state.MerchantCardTrack.Count > 0)
            {
                yield return new AcquireMove(state);
            }
        }

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];
            var acquired = state.MerchantCardTrack[state.MerchantCardIndexAfforded];

            pInventory = pInventory.With(
                hand: pInventory.Hand.Add(acquired.MerchantCard),
                caravan: pInventory.Caravan.AddRange(acquired.Spices));

            var merchantCardDeck = state.MerchantCardDeck.Deal(1, out ImmutableList<MerchantCard> dealt);
            var merchantCardTrack = state.MerchantCardTrack
                .RemoveAt(state.MerchantCardIndexAfforded)
                .AddRange(dealt.Select(c => new MerchantStall(c, EnumCollection<Spice>.Empty)));

            state = state.With(
                merchantCardDeck: merchantCardDeck,
                merchantCardTrack: merchantCardTrack,
                inventory: state.Inventory.SetItem(activePlayer, pInventory),
                merchantCardIndexAfforded: 0,
                phase: Phase.Play);

            return base.Apply(state);
        }
    }
}
