// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Cards
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the Revolution card.
    /// </summary>
    public sealed class RevolutionCard : Card
    {
        /// <summary>
        /// The single instance of the <see cref="RevolutionCard"/> class.
        /// </summary>
        public static readonly RevolutionCard Instance = new RevolutionCard();

        private RevolutionCard()
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.Revolution };

        /// <inheritdoc/>
        public override int CompareTo(Card other)
        {
            if (other is RevolutionCard)
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
