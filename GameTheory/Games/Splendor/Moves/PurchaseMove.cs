// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    /// <summary>
    /// Represents a move to reserve a development card from the board or the player's hand.
    /// </summary>
    public class PurchaseMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="card">The development card to purchase.</param>
        public PurchaseMove(GameState state, DevelopmentCard card)
            : base(state)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the development card to purchase.
        /// </summary>
        public DevelopmentCard Card { get; }

        /// <inheritdoc />
        public override string ToString() => $"Purchase {this.Card} for {string.Join(",", this.Card.Cost)}";

        internal override GameState Apply(GameState state)
        {
            return base.Apply(state);
        }
    }
}
