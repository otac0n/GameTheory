﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to take all of the <see cref="Meeple">Meeples</see> in hand, and put them in the active player's inventory.
    /// </summary>
    public sealed class TakeMeeplesInHandMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeMeeplesInHandMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public TakeMeeplesInHandMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.TakeItem, this.GameState.InHand);

        /// <inheritdoc />
        public override bool IsDeterministic => true;

        internal static IEnumerable<TakeMeeplesInHandMove> GenerateMoves(GameState state)
        {
            yield return new TakeMeeplesInHandMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            var player = state.ActivePlayer;
            var inventory = state.Inventory[player];
            return state.With(
                inHand: EnumCollection<Meeple>.Empty,
                inventory: state.Inventory.SetItem(player, inventory.With(meeples: inventory.Meeples.AddRange(state.InHand))),
                phase: Phase.TileAction);
        }
    }
}
