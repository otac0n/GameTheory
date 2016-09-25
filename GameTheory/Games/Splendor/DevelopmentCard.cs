// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    /// <summary>
    /// Describes a development card.
    /// </summary>
    public class DevelopmentCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentCard"/> class.
        /// </summary>
        /// <param name="prestige">The prestige awarded by this card.</param>
        /// <param name="cost">The cost of this card.</param>
        public DevelopmentCard(int prestige, EnumCollection<Token> cost)
        {
            this.Prestige = prestige;
            this.Cost = cost;
        }

        /// <summary>
        /// Gets the cost of this card.
        /// </summary>
        public EnumCollection<Token> Cost { get; }

        /// <summary>
        /// Gets the prestige awarded by this card.
        /// </summary>
        public int Prestige { get; }
    }
}
