// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a move to acquire a merchant card.
    /// </summary>
    public sealed class AcquireMove : Move
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
        public override IList<object> FormatTokens
        {
            get
            {
                var merchantStall = this.GameState.MerchantCardTrack[this.GameState.MerchantCardIndexAfforded];
                return merchantStall.Spices.Count > 0
                    ? FormatUtilities.ParseStringFormat(Resources.AcquireMerchantCardAndBonus, merchantStall.MerchantCard, merchantStall.Spices)
                    : FormatUtilities.ParseStringFormat(Resources.AcquireMerchantCard, merchantStall.MerchantCard);
            }
        }

        /// <inheritdoc />
        public override bool IsDeterministic => this.GameState.MerchantCardDeck.Count <= 1;

        internal static IEnumerable<AcquireMove> GenerateMoves(GameState state)
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

            var inventory = state.Inventory.SetItem(activePlayer, pInventory);
            var merchantCardTrack = state.MerchantCardTrack.RemoveAt(state.MerchantCardIndexAfforded);

            var merchantCardDeck = state.MerchantCardDeck.Deal(1, out var dealt);
            merchantCardTrack = merchantCardTrack.AddRange(dealt.Select(c => new MerchantStall(c, EnumCollection<Spice>.Empty)));

            state = state.With(
                merchantCardDeck: merchantCardDeck,
                merchantCardTrack: merchantCardTrack,
                inventory: inventory,
                merchantCardIndexAfforded: 0,
                phase: Phase.Play);

            return base.Apply(state);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];
            var acquired = state.MerchantCardTrack[state.MerchantCardIndexAfforded];

            pInventory = pInventory.With(
                hand: pInventory.Hand.Add(acquired.MerchantCard),
                caravan: pInventory.Caravan.AddRange(acquired.Spices));

            var inventory = state.Inventory.SetItem(activePlayer, pInventory);
            var merchantCardTrack = state.MerchantCardTrack.RemoveAt(state.MerchantCardIndexAfforded);

            if (state.MerchantCardDeck.Count == 0)
            {
                var outcome = base.Apply(state.With(
                    merchantCardTrack: merchantCardTrack,
                    inventory: inventory,
                    merchantCardIndexAfforded: 0,
                    phase: Phase.Play));
                yield return Weighted.Create(outcome, 1);
            }
            else
            {
                for (var i = 0; i < state.MerchantCardDeck.Count; i++)
                {
                    var outcome = base.Apply(state.With(
                        merchantCardDeck: state.MerchantCardDeck.RemoveAt(i),
                        merchantCardTrack: merchantCardTrack.Add(new MerchantStall(state.MerchantCardDeck[i], EnumCollection<Spice>.Empty)),
                        inventory: inventory,
                        merchantCardIndexAfforded: 0,
                        phase: Phase.Play));
                    yield return Weighted.Create(outcome, 1);
                }
            }
        }
    }
}
