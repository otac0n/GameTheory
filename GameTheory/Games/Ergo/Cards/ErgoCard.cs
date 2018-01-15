// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Cards
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the Ergo card.
    /// </summary>
    public sealed class ErgoCard : Card
    {
        /// <summary>
        /// The single instance of the <see cref="ErgoCard"/> class.
        /// </summary>
        public static readonly ErgoCard Instance = new ErgoCard();

        private ErgoCard()
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { "Ergo" };

        /// <inheritdoc/>
        public override int CompareTo(Card other)
        {
            if (other is ErgoCard)
            {
                return 0;
            }
            else
            {
                return base.CompareTo(other);
            }
        }
    }
}
