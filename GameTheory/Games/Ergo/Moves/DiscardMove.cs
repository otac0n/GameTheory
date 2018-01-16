// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a move to discard a card.
    /// </summary>
    public sealed class DiscardMove : Move
    {
        private DiscardMove(GameState state, Card card)
            : base(state)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the card to be discarded.
        /// </summary>
        public Card Card { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Discard ", this.Card };

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is DiscardMove discard)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(discard.PlayerToken)) != 0 ||
                    (comp = this.Card.CompareTo(discard.Card)) != 0)
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

        internal static IEnumerable<DiscardMove> GenerateMoves(GameState state)
        {
            foreach (var card in state.Hands[state.ActivePlayer].Distinct())
            {
                yield return new DiscardMove(state, card);
            }
        }

        internal override GameState Apply(GameState state)
        {
            var hand = state.Hands[this.PlayerToken];

            hand = hand.Remove(this.Card);

            state = state.With(
                hands: state.Hands.SetItem(this.PlayerToken, hand));

            return base.Apply(state);
        }
    }
}
