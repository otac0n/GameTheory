// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.MerchantCards
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a spice producing card.
    /// </summary>
    public class SpiceCard : MerchantCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceCard"/> class.
        /// </summary>
        /// <param name="spices">The spices produced by this card.</param>
        public SpiceCard(EnumCollection<Spice> spices)
        {
            this.Spices = spices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceCard"/> class.
        /// </summary>
        /// <param name="turmeric">The turmeric produced.</param>
        /// <param name="saffron">The saffron produced.</param>
        /// <param name="cardamom">The cardamom produced.</param>
        /// <param name="cinnamon">The cinnamon produced.</param>
        public SpiceCard(int turmeric = 0, int saffron = 0, int cardamom = 0, int cinnamon = 0)
            : this(EnumCollection<Spice>.Empty
                  .Add(Spice.Turmeric, turmeric)
                  .Add(Spice.Saffron, saffron)
                  .Add(Spice.Cardamom, cardamom)
                  .Add(Spice.Cinnamon, cinnamon))
        {
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.GainSpices, this.Spices);

        /// <summary>
        /// Gets the spices produced by this card.
        /// </summary>
        public EnumCollection<Spice> Spices { get; }

        /// <inheritdoc/>
        public override int CompareTo(MerchantCard other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (other is SpiceCard spiceCard)
            {
                return this.Spices.CompareTo(spiceCard.Spices);
            }
            else
            {
                return base.CompareTo(other);
            }
        }
    }
}
