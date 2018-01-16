// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a move to draw cards.
    /// </summary>
    public sealed class DrawCardsMove : Move
    {
        private DrawCardsMove(GameState state)
            : base(state)
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Draw cards" };

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DrawCardsMove drawCards)
            {
                return this.PlayerToken.CompareTo(drawCards.PlayerToken);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        internal static IEnumerable<DrawCardsMove> GenerateMoves(GameState state)
        {
            yield return new DrawCardsMove(state);
        }

        internal override GameState Apply(GameState state)
        {
            var deck = state.Deck;
            var hand = state.Hands[this.PlayerToken];

            var toDeal = Math.Min(deck.Count, 2);
            hand = hand.AddRange(deck.GetRange(deck.Count - toDeal, toDeal));
            deck = deck.GetRange(0, deck.Count - toDeal);

            state = state.With(
                deck: deck,
                hands: state.Hands.SetItem(this.PlayerToken, hand));

            return base.Apply(state);
        }
    }
}
