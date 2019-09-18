// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Cards
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the Justification card.
    /// </summary>
    public sealed class JustificationCard : Card
    {
        /// <summary>
        /// The single instance of the <see cref="JustificationCard"/> class.
        /// </summary>
        public static readonly JustificationCard Instance = new JustificationCard();

        private JustificationCard()
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.Justification };

        /// <inheritdoc/>
        public override int CompareTo(Card other)
        {
            if (other is JustificationCard)
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
