// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Moves
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        public override bool IsDeterministic => this.GameState.Deck.Keys.Count() <= 1;

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
            var hidden = state.Hidden;

            if (deck.Count > 0)
            {
                deck = deck.Deal(out var dealt);
                hand = hand.Add(dealt);
            }
            else if (hidden != Card.None)
            {
                hand = hand.Add(hidden);
                hidden = Card.None;
            }

            activePlayerInventory = activePlayerInventory.With(
                hand: hand);
            inventory = inventory.SetItem(
                activePlayer,
                activePlayerInventory);

            state = state.With(
                hidden: hidden,
                inventory: inventory,
                deck: deck);

            return base.Apply(state);
        }

        internal override IEnumerable<IWeighted<GameState>> GetOutcomes(GameState state)
        {
            throw new NotImplementedException();
        }
    }
}
