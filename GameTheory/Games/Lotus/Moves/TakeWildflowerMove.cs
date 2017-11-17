// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to take a wildflower.
    /// </summary>
    public class TakeWildflowerMove : Move
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
        public override IList<object> FormatTokens => new object[] { "Take ", this.State.AvailableWildflowers[this.WildflowerIndex] };

        /// <inheritdoc />
        public override bool IsDeterministic => false;

        /// <summary>
        /// Gets the index of the wildflower to take.
        /// </summary>
        public int WildflowerIndex { get; }

        internal static IEnumerable<Move> GenerateMoves(GameState state)
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
