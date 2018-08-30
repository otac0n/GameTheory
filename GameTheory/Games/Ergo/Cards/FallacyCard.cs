// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Cards
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the Fallacy card.
    /// </summary>
    public sealed class FallacyCard : Card
    {
        /// <summary>
        /// The single instance of the <see cref="FallacyCard"/> class.
        /// </summary>
        public static readonly FallacyCard Instance = new FallacyCard();

        private FallacyCard()
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.Fallacy };

        /// <inheritdoc/>
        public override int CompareTo(Card other)
        {
            if (other is FallacyCard)
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
