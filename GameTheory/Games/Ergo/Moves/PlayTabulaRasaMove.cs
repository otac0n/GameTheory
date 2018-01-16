// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents a move to remove a card by playing a Tabula Rasa card.
    /// </summary>
    public class PlayTabulaRasaMove : Move
    {
        private PlayTabulaRasaMove(GameState state, TabulaRasaCard card, int premiseIndex, int removeIndex)
            : base(state)
        {
            this.Card = card;
            this.PremiseIndex = premiseIndex;
            this.RemoveIndex = removeIndex;
        }

        /// <summary>
        /// Gets the card that will be played.
        /// </summary>
        public TabulaRasaCard Card { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Remove at ", this.PremiseIndex, ":", this.RemoveIndex, " using ", this.Card };

        /// <summary>
        /// Gets the premise from which a card will be removed.
        /// </summary>
        public int PremiseIndex { get; }

        /// <summary>
        /// Gets the index of the card that will be removed.
        /// </summary>
        public int RemoveIndex { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlayTabulaRasaMove tabulaRasa)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(tabulaRasa.PlayerToken)) != 0 ||
                    (comp = this.PremiseIndex.CompareTo(tabulaRasa.PremiseIndex)) != 0 ||
                    (comp = this.RemoveIndex.CompareTo(tabulaRasa.RemoveIndex)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Proof[this.PremiseIndex], tabulaRasa.GameState.Proof[tabulaRasa.PremiseIndex])) != 0)
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

        internal static IEnumerable<PlayTabulaRasaMove> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            if (state.FallacyCounter[activePlayer] <= 0 && state.Hands[activePlayer].Contains(TabulaRasaCard.Instance))
            {
                for (var p = 0; p < state.Proof.Count; p++)
                {
                    var premise = state.Proof[p];
                    for (var i = 0; i < premise.Count; i++)
                    {
                        yield return new PlayTabulaRasaMove(state, TabulaRasaCard.Instance, p, i);
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var deck = state.Deck;
            var premise = state.Proof[this.PremiseIndex];
            var hand = state.Hands[this.PlayerToken];

            deck = deck.Insert(0, premise[this.RemoveIndex].Card);
            premise = premise.RemoveAt(this.RemoveIndex);
            hand = hand.Remove(this.Card);

            state = state.With(
                deck: deck,
                hands: state.Hands.SetItem(this.PlayerToken, hand),
                proof: state.Proof.SetItem(this.PremiseIndex, premise));

            return base.Apply(state);
        }
    }
}
