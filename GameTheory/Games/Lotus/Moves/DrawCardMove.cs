// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to draw a card from the player deck.
    /// </summary>
    public class DrawCardMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        public DrawCardMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => new object[] { "Draw a card" };

        internal static IEnumerable<Move> GenerateMoves(GameState state)
        {
            if (state.Inventory[state.ActivePlayer].Deck.Count > 0)
            {
                yield return new DrawCardMove(state);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];

            var dealtIndex = playerInventory.Deck.Count - 1;
            var dealt = playerInventory.Deck[dealtIndex];
            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.Add(dealt),
                deck: playerInventory.Deck.RemoveAt(dealtIndex));

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory));

            return base.Apply(state);
        }
    }
}
