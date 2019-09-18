// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents a move to end the round using an Ergo card.
    /// </summary>
    public class PlayErgoMove : Move
    {
        static PlayErgoMove()
        {
            ErgoCards = ImmutableArray.Create<Card>(ErgoCard.Instance, SymbolCard.WildOperator, SymbolCard.WildVariable);
        }

        private PlayErgoMove(GameState state, Card card)
            : base(state)
        {
            this.Card = card;
        }

        /// <summary>
        /// Gets the card that will be played.
        /// </summary>
        public Card Card { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => this.Card == ErgoCard.Instance
            ? FormatUtilities.ParseStringFormat(Resources.PlayCard, this.Card)
            : FormatUtilities.ParseStringFormat(Resources.PlayCardAs, this.Card, ErgoCard.Instance);

        private static ImmutableArray<Card> ErgoCards { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlayErgoMove ergo)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(ergo.PlayerToken)) != 0 ||
                    (comp = this.Card.CompareTo(ergo.Card)) != 0)
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

        internal static IEnumerable<PlayErgoMove> GenerateMoves(GameState state)
        {
            foreach (var ergo in ErgoCards)
            {
                if (state.Hands[state.ActivePlayer].Contains(ergo))
                {
                    var needed = new HashSet<Symbol>(SymbolCard.WildVariable.Symbols);
                    needed.ExceptWith(state.Proof.SelectMany(p => p.Select(c => c.Symbol)));
                    if (needed.Count == 0)
                    {
                        yield return new PlayErgoMove(state, ergo);
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
                isRoundOver: true);

            return base.Apply(state);
        }
    }
}
