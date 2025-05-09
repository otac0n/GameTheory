﻿// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to take a wildflower.
    /// </summary>
    public sealed class TakeWildflowerMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeWildflowerMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="wildflowerIndex">The index of the wildflower to take.</param>
        public TakeWildflowerMove(GameState state, int wildflowerIndex)
            : base(state)
        {
            this.WildflowerIndex = wildflowerIndex;
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens =>
            FormatUtilities.ParseStringFormat(Resources.TakeWildflowerFormat, this.GameState.AvailableWildflowers[this.WildflowerIndex]);

        /// <summary>
        /// Gets the index of the wildflower to take.
        /// </summary>
        public int WildflowerIndex { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is TakeWildflowerMove move)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(other.PlayerToken)) != 0 ||
                    (comp = this.WildflowerIndex.CompareTo(move.WildflowerIndex)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.AvailableWildflowers, move.GameState.AvailableWildflowers)) != 0)
                {
                    return comp;
                }

                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<TakeWildflowerMove> GenerateMoves(GameState state)
        {
            for (var i = 0; i < state.AvailableWildflowers.Count; i++)
            {
                if (state.AvailableWildflowers[i] != null)
                {
                    yield return new TakeWildflowerMove(state, i);
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory.With(
                        hand: playerInventory.Hand.Add(state.AvailableWildflowers[this.WildflowerIndex]))),
                availableWildflowers: state.AvailableWildflowers.SetItem(this.WildflowerIndex, null));

            return base.Apply(state);
        }
    }
}
