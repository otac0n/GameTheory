// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a rest move.
    /// </summary>
    public sealed class RestMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public RestMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { Resources.Rest };

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal static IEnumerable<RestMove> GenerateMoves(GameState state)
        {
            if (state.Inventory[state.ActivePlayer].PlayedCards.Count > 0)
            {
                yield return new RestMove(state);
            }
        }

        /// <inheritdoc />
        internal override GameState Apply(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            var pInventory = state.Inventory[activePlayer];

            pInventory = pInventory.With(
                hand: pInventory.Hand.AddRange(pInventory.PlayedCards),
                playedCards: ImmutableList<MerchantCard>.Empty);

            state = state.With(
                inventory: state.Inventory.SetItem(activePlayer, pInventory));

            return base.Apply(state);
        }
    }
}
