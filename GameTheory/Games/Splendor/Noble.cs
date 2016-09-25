// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    /// <summary>
    /// Describes a Noble.
    /// </summary>
    public class Noble
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Noble"/> class.
        /// </summary>
        /// <param name="requiredBonuses">The bonuses required for this Noble to visit.</param>
        public Noble(EnumCollection<Token> requiredBonuses)
        {
            this.RequiredBonuses = requiredBonuses;
        }

        /// <summary>
        /// Gets the prestige awarded by this Noble.
        /// </summary>
        public int Prestige => 3;

        /// <summary>
        /// Gets the bonuses required for this Noble to visit.
        /// </summary>
        public EnumCollection<Token> RequiredBonuses { get; }
    }
}
