// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Moves
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents a move to place a symbol.
    /// </summary>
    public sealed class PlaceSymbolMove : Move
    {
        private PlaceSymbolMove(GameState state, SymbolCard card, int symbolIndex, int premiseIndex, int insertIndex)
            : base(state)
        {
            this.Card = card;
            this.SymbolIndex = symbolIndex;
            this.PremiseIndex = premiseIndex;
            this.InsertIndex = insertIndex;
        }

        /// <summary>
        /// Gets the card that will be played.
        /// </summary>
        public SymbolCard Card { get; }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => this.Card.Symbols.Count == 1
                ? FormatUtilities.ParseStringFormat(Resources.PlaceCard, this.Card.Symbols[this.SymbolIndex], this.PremiseIndex, this.InsertIndex)
                : FormatUtilities.ParseStringFormat(Resources.PlaceCardAs, this.Card, this.Card.Symbols[this.SymbolIndex], this.PremiseIndex, this.InsertIndex);

        /// <summary>
        /// Gets the index at which the card will be inserted.
        /// </summary>
        public int InsertIndex { get; }

        /// <summary>
        /// Gets the premise to which a card will be added.
        /// </summary>
        public int PremiseIndex { get; }

        /// <summary>
        /// Gets the index on the card of the symbol to be added.
        /// </summary>
        public int SymbolIndex { get; }

        /// <inheritdoc />
        public override int CompareTo(Move other)
        {
            if (other is PlaceSymbolMove placeSymbol)
            {
                int comp;

                if ((comp = this.PlayerToken.CompareTo(placeSymbol.PlayerToken)) != 0 ||
                    (comp = this.SymbolIndex.CompareTo(placeSymbol.SymbolIndex)) != 0 ||
                    (comp = this.PremiseIndex.CompareTo(placeSymbol.PremiseIndex)) != 0 ||
                    (comp = this.InsertIndex.CompareTo(placeSymbol.InsertIndex)) != 0 ||
                    (comp = this.Card.CompareTo(placeSymbol.Card)) != 0 ||
                    (comp = CompareUtilities.CompareLists(this.GameState.Proof[this.PremiseIndex], placeSymbol.GameState.Proof[placeSymbol.PremiseIndex])) != 0)
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

        internal static IEnumerable<PlaceSymbolMove> GenerateMoves(GameState state)
        {
            var activePlayer = state.ActivePlayer;
            if (state.FallacyCounter[activePlayer] <= 0)
            {
                foreach (var card in state.Hands[activePlayer].OfType<SymbolCard>().Distinct())
                {
                    for (var p = 0; p < state.Proof.Count; p++)
                    {
                        var premise = state.Proof[p];
                        for (var i = 0; i <= premise.Count; i++)
                        {
                            for (var s = 0; s < card.Symbols.Count; s++)
                            {
                                yield return new PlaceSymbolMove(state, card, s, p, i);
                            }
                        }
                    }
                }
            }
        }

        internal override GameState Apply(GameState state)
        {
            var proof = state.Proof;
            var hand = state.Hands[this.PlayerToken];

            proof = proof.SetItem(this.PremiseIndex, proof[this.PremiseIndex].Insert(this.InsertIndex, new PlacedCard(this.Card, this.SymbolIndex)));
            hand = hand.Remove(this.Card);

            state = state.With(
                hands: state.Hands.SetItem(this.PlayerToken, hand),
                proof: proof);

            return base.Apply(state);
        }
    }
}
