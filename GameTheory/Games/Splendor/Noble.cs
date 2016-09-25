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
        /// Initializes a new instance of the <see cref="Noble"/> class.
        /// </summary>
        public Noble(int diamond = 0, int sapphire = 0, int emerald = 0, int ruby = 0,  int onyx = 0)
            : this(EnumCollection<Token>.Empty
                  .Add(Token.Diamond, diamond)
                  .Add(Token.Sapphire, sapphire)
                  .Add(Token.Emerald, emerald)
                  .Add(Token.Ruby, ruby)
                  .Add(Token.Onyx, onyx))
        {
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
