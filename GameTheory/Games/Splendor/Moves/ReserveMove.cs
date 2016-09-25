// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor.Moves
{
    /// <summary>
    /// Represents a move to reserve a development card from the board.
    /// </summary>
    public class ReserveMove : Move
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReserveMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="card">The development card to reserve.</param>
        public ReserveMove(GameState state, DevelopmentCard card)
            : base(state)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the development card to reserve.
        /// </summary>
        public DevelopmentCard Card { get; }

        /// <inheritdoc />
        public override string ToString() => $"Reserve {this.Card}" + (this.State.Tokens[Token.GoldJoker] > 0 ? $" and take {Token.GoldJoker}" : string.Empty);

        internal override GameState Apply(GameState state)
        {
            var tokens = state.Tokens;
            var pInventory = state.Inventory[state.ActivePlayer];
            var pHand = pInventory.Hand;
            var pTokens = pInventory.Tokens;

            // TODO: Replace card with one from deck or null.
            pHand = pHand.Add(this.Card);

            if (tokens[Token.GoldJoker] > 0)
            {
                tokens = tokens.Remove(Token.GoldJoker);
                pTokens = pTokens.Add(Token.GoldJoker);
            }

            return base.Apply(state.With(
                tokens: tokens,
                inventory: state.Inventory.SetItem(state.ActivePlayer, pInventory.With(
                    hand: pHand,
                    tokens: pTokens))));
        }
    }
}
