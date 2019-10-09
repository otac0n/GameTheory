// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to draw a card.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCardMove"/> class.
        /// </summary>
        /// <param name="state">The <see cref="GameState"/> that this move is based on.</param>
        /// <param name="player">The player drawing a card.</param>
        public DrawCardMove(GameState state, PlayerToken player)
            : base(state, player)
        {
        }

        /// <inheritdoc />
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.DrawCard);

        /// <inheritdoc />
        public override bool IsDeterministic => this.GameState.Deck.Count <= 1;

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }

            if (other is DrawCardMove move)
            {
                return this.PlayerToken.CompareTo(move.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<DrawCardMove> GenerateMoves(GameState state)
        {
            yield return new DrawCardMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            var inventory = state.Inventory;

            var deck = state.Deck;
            var activePlayer = this.PlayerToken;
            var activePlayerInventory = inventory[activePlayer];
            var hand = activePlayerInventory.Hand;

            deck = deck.Deal(out var dealt);
            hand = hand.Add(dealt);
            activePlayerInventory = activePlayerInventory.With(
                hand: hand);
            inventory = inventory.SetItem(
                activePlayer,
                activePlayerInventory);

            state = state.With(
                inventory: inventory,
                deck: deck);

            return base.Apply(state);
        }
    }
}
