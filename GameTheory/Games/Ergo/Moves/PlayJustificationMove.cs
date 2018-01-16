// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents a move to remove fallacy by playing a Justification card.
    /// </summary>
    public class PlayJustificationMove : Move
    {
        static PlayJustificationMove()
        {
            JustificationCards = ImmutableArray.Create<Card>(JustificationCard.Instance, SymbolCard.WildOperator, SymbolCard.WildVariable);
        }

        private PlayJustificationMove(GameState state, Card card)
            : base(state)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the card that will be played.
        /// </summary>
        public Card Card { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Remove fallacy with ", this.Card };

        private static ImmutableArray<Card> JustificationCards { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlayJustificationMove justification)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(justification.PlayerToken)) != 0 ||
                    (comp = this.Card.CompareTo(justification.Card)) != 0)
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

        internal static IEnumerable<PlayJustificationMove> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            if (state.FallacyCounter[activePlayer] > 0)
            {
                foreach (var justification in JustificationCards)
                {
                    if (state.Hands[activePlayer].Contains(justification))
                    {
                        yield return new PlayJustificationMove(state, justification);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var hand = state.Hands[this.PlayerToken];

            hand = hand.Remove(this.Card);

            state = state.With(
                hands: state.Hands.SetItem(this.PlayerToken, hand),
                fallacyCounter: state.FallacyCounter.SetItem(this.PlayerToken, -1));

            return base.Apply(state);
        }
    }
}
