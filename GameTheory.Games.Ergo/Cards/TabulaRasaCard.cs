// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Ergo.Cards
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the Tabula Rasa card.
    /// </summary>
    public sealed class TabulaRasaCard : Card
    {
        /// <summary>
        /// The single instance of the <see cref="TabulaRasaCard"/> class.
        /// </summary>
        public static readonly TabulaRasaCard Instance = new TabulaRasaCard();

        private TabulaRasaCard()
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => new object[] { Resources.TabulaRasa };

        /// <inheritdoc/>
        public override int CompareTo(Card other)
        {
            if (other is TabulaRasaCard)
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
