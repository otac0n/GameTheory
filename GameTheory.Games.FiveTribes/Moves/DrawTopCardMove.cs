﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Represents a move to draw the top card from the <see cref="Resource"/> pile.
    /// </summary>
    public sealed class DrawTopCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawTopCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DrawTopCardMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { Resources.DrawTopResource };

        /// <inheritdoc />
        public override bool IsDeterministic => false;

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];

            var newDiscards = state.ResourceDiscards;
            var newResourcesPile = state.ResourcePile.Deal(1, out var dealt, ref newDiscards);
            var newInventory = inventory.With(resources: inventory.Resources.AddRange(dealt));

            return state.With(
                inventory: state.Inventory.SetItem(player, newInventory),
                resourceDiscards: newDiscards,
                resourcePile: newResourcesPile);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            // TODO: Implement outcomes.
            return base.GetOutcomes(state);
        }
    }
}
