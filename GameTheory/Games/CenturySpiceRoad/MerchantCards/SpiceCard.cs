﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.MerchantCards
{
    public class SpiceCard : MerchantCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceCard"/> class.
        /// </summary>
        /// <param name="spices">The spices awarded by this card.</param>
        public SpiceCard(EnumCollection<Spice> spices)
        {
            this.Spices = spices;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceCard"/> class.
        /// </summary>
        /// <param name="turmeric">The turmeric component of the reward.</param>
        /// <param name="safran">The safran component of the reward.</param>
        /// <param name="cardamom">The cardamom component of the reward.</param>
        /// <param name="cinnamon">The cinnamon component of the reward.</param>
        public SpiceCard(int turmeric = 0, int safran = 0, int cardamom = 0, int cinnamon = 0)
            : this(EnumCollection<Spice>.Empty
                  .Add(Spice.Turmeric, turmeric)
                  .Add(Spice.Safran, safran)
                  .Add(Spice.Cardamom, cardamom)
                  .Add(Spice.Cinnamon, cinnamon))
        {
        }

        /// <summary>
        /// Gets the spices awarded by this card.
        /// </summary>
        public EnumCollection<Spice> Spices { get; }

        /// <inheritdoc/>
        public override int CompareTo(MerchantCard other)
        {
            if (other == this)
            {
                return 0;
            }
            else if (other == null)
            {
                return 1;
            }

            var spiceCard = other as SpiceCard;
            if (spiceCard == null)
            {
                return base.CompareTo(other);
            }

            return this.Spices.CompareTo(spiceCard.Spices);
        }

        /// <inheritdoc/>
        public override string ToString() => $"Gain {this.Spices}";
    }
}
