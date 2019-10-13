// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to draw a card from the player deck.
    /// </summary>
    public sealed class DrawCardMove : Move
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
        public override IList<object> FormatTokens => new object[] { Resources.DrawCard };

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DrawCardMove)
            {
                return this.PlayerToken.CompareTo(other.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<DrawCardMove> GenerateMoves(GameState state)
        {
            if (state.Deck.Count > 0)
            {
                yield return new DrawCardMove(state);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var playerInventory = state.Inventory[this.PlayerToken];


            var deck = state.Deck.Deal(out var dealt);
            playerInventory = playerInventory.With(
                hand: playerInventory.Hand.Add(dealt));

            state = state.With(
                inventory: state.Inventory.SetItem(
                    this.PlayerToken,
                    playerInventory),
                deck: deck);

            return base.Apply(state);
        }
    }
}
