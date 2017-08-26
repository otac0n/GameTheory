// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;

    /// <summary>
    /// A merchant stall represents the in-game unit of a card and the spices placed atop.
    /// </summary>
    public class MerchantStall : IComparable<MerchantStall>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantStall"/> class.
        /// </summary>
        /// <param name="merchantCard">The <see cref="MerchantCard"/> at this <see cref="MerchantStall"/>.</param>
        /// <param name="spices">The <see cref="Spice">Spices</see> at this <see cref="MerchantStall"/>.</param>
        public MerchantStall(MerchantCard merchantCard, EnumCollection<Spice> spices)
        {
            this.MerchantCard = merchantCard;
            this.Spices = spices;
        }

        /// <summary>
        /// Gets the <see cref="Spice">Spices</see> at this <see cref="MerchantStall"/>.
        /// </summary>
        public EnumCollection<Spice> Spices { get; }

        /// <summary>
        /// Gets the <see cref="MerchantCard"/> at this <see cref="MerchantStall"/>.
        /// </summary>
        public MerchantCard MerchantCard { get; }

        /// <summary>
        /// Creates a new <see cref="MerchantStall"/>, and updates the specified values.
        /// </summary>
        /// <param name="merchantCard"><c>null</c> to keep the existing value, or any other value to update <see cref="MerchantCard"/>.</param>
        /// <param name="spices"><c>null</c> to keep the existing value, or any other value to update <see cref="Spices"/>.</param>
        /// <returns>The new <see cref="MerchantStall"/>.</returns>
        public MerchantStall With(MerchantCard merchantCard = null, EnumCollection<Spice> spices = null)
        {
            return new MerchantStall(
                merchantCard ?? this.MerchantCard,
                spices ?? this.Spices);
        }

        /// <inheritdoc/>
        public int CompareTo(MerchantStall other)
        {
            if (other == this)
            {
                return 0;
            }
            else if (other == null)
            {
                return 1;
            }

            int comp;

            if ((comp = this.MerchantCard.CompareTo(other.MerchantCard)) != 0 ||
                (comp = this.Spices.CompareTo(other.Spices)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override string ToString() => this.Spices.Count == 0 ? $"{this.MerchantCard}" : $"{this.MerchantCard} with {this.Spices}";
    }
}
