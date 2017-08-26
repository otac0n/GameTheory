// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.MerchantCards
{
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

        /// <summary>
        /// Gets the upgrade amount of the card.
        /// </summary>
        public int UpgradeLevel { get; }

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

            var upgradeCard = other as UpgradeCard;
            if (upgradeCard == null)
            {
                return base.CompareTo(other);
            }

            return this.UpgradeLevel.CompareTo(upgradeCard.UpgradeLevel);
        }

        /// <inheritdoc/>
        public override string ToString() => $"Upgrade ({this.UpgradeLevel})";
    }
}
