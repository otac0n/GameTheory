// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.MerchantCards
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents an upgrade card.
    /// </summary>
    public class UpgradeCard : MerchantCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpgradeCard"/> class.
        /// </summary>
        /// <param name="upgradeLevel">The upgrade level of this card.</param>
        public UpgradeCard(int upgradeLevel)
        {
            this.UpgradeLevel = upgradeLevel;
        }

        /// <inheritdoc/>
        public override IList<object> FormatTokens => FormatUtilities.ParseStringFormat(Resources.UpgradeCard, this.UpgradeLevel);

        /// <summary>
        /// Gets the upgrade amount of the card.
        /// </summary>
        public int UpgradeLevel { get; }

        /// <inheritdoc/>
        public override int CompareTo(MerchantCard other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (other is UpgradeCard upgradeCard)
            {
                return this.UpgradeLevel.CompareTo(upgradeCard.UpgradeLevel);
            }
            else
            {
                return base.CompareTo(other);
            }
        }
    }
}
