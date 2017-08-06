// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad.MerchantCards
{
    public class TradeCard : MerchantCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeCard"/> class.
        /// </summary>
        /// <param name="cost">The cost of this card.</param>
        /// <param name="reward">The spices awarded by this card.</param>
        public TradeCard(EnumCollection<Spice> cost, EnumCollection<Spice> reward)
        {
            this.Cost = cost;
            this.Reward = reward;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeCard"/> class.
        /// </summary>
        /// <param name="turmericCost">The turmeric component of the cost.</param>
        /// <param name="safranCost">The safran component of the cost.</param>
        /// <param name="cardamomCost">The cardamom component of the cost.</param>
        /// <param name="cinnamonCost">The cinnamon component of the cost.</param>
        /// <param name="turmericReward">The turmeric component of the reward.</param>
        /// <param name="safranReward">The safran component of the reward.</param>
        /// <param name="cardamomReward">The cardamom component of the reward.</param>
        /// <param name="cinnamonReward">The cinnamon component of the reward.</param>
        public TradeCard(int turmericCost = 0, int safranCost = 0, int cardamomCost = 0, int cinnamonCost = 0, int turmericReward = 0, int safranReward = 0, int cardamomReward = 0, int cinnamonReward = 0)
            : this(
                EnumCollection<Spice>.Empty.Add(Spice.Turmeric, turmericCost).Add(Spice.Safran, safranCost).Add(Spice.Cardamom, cardamomCost).Add(Spice.Cinnamon, cinnamonCost),
                EnumCollection<Spice>.Empty.Add(Spice.Turmeric, turmericReward).Add(Spice.Safran, safranReward).Add(Spice.Cardamom, cardamomReward).Add(Spice.Cinnamon, cinnamonReward))
        {
        }

        /// <summary>
        /// Gets the cost of this card.
        /// </summary>
        public EnumCollection<Spice> Cost { get; }

        /// <summary>
        /// Gets the spices awarded by this card.
        /// </summary>
        public EnumCollection<Spice> Reward { get; }

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

            var tradeCard = other as TradeCard;
            if (tradeCard == null)
            {
                return base.CompareTo(other);
            }

            int comp;

            if ((comp = this.Cost.CompareTo(tradeCard.Cost)) != 0 ||
                (comp = this.Reward.CompareTo(tradeCard.Reward)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override string ToString() => $"Trade {this.Cost} for {this.Reward}";
    }
}
