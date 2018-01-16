// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo
{
    using System;
    using System.Collections.Generic;
    using GameTheory.Games.Ergo.Cards;

    /// <summary>
    /// Represents a card that was placed in the premise.
    /// </summary>
    public class PlacedCard : IComparable<PlacedCard>, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlacedCard"/> class.
        /// </summary>
        /// <param name="card">The card that was played.</param>
        /// <param name="symbolIndex">The index of the symbol that was played.</param>
        public PlacedCard(SymbolCard card, int symbolIndex)
        {
            this.Card = card;
            this.SymbolIndex = symbolIndex;
        }

        /// <summary>
        /// Gets the card that was played.
        /// </summary>
        public SymbolCard Card { get; }

        /// <inheritdoc/>
        public IList<object> FormatTokens => new object[] { this.Symbol };

        /// <summary>
        /// Gets the symbol from the card that was played.
        /// </summary>
        public Symbol Symbol => this.Card.Symbols[this.SymbolIndex];

        /// <summary>
        /// Gets the index of the symbol that was played.
        /// </summary>
        public int SymbolIndex { get; }

        /// <inheritdoc/>
        public int CompareTo(PlacedCard other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int comp;

            if ((comp = this.SymbolIndex.CompareTo(other.SymbolIndex)) != 0 ||
                (comp = this.Card.CompareTo(other.Card)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
