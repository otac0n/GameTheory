// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    /// <summary>
    /// Holds state regarding the assassination process.
    /// </summary>
    public class AssassinationTable
    {
        private readonly bool hasProtection;
        private readonly int killCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinationTable"/> class.
        /// </summary>
        public AssassinationTable()
            : this(false, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinationTable"/> class.
        /// </summary>
        /// <param name="hasProtection">A value indicating whether or not the player's <see cref="Meeple">Meeples</see> have protection from assassination.</param>
        /// <param name="killCount">A number corresponding to the number of <see cref="Meeple">Meeples</see> that the player can kill during an assassination.</param>
        public AssassinationTable(bool hasProtection, int killCount)
        {
            this.hasProtection = hasProtection;
            this.killCount = killCount;
        }

        /// <summary>
        /// Gets a value indicating whether or not the player's <see cref="Meeple">Meeples</see> have protection from assassination.
        /// </summary>
        public bool HasProtection
        {
            get { return this.hasProtection; }
        }

        /// <summary>
        /// Gets a number corresponding to the number of meeples that the player can kill during an assassination.
        /// </summary>
        public int KillCount
        {
            get { return this.killCount; }
        }

        /// <summary>
        /// Creates a new <see cref="AssassinationTable"/>, and updates the specified values.
        /// </summary>
        /// <param name="hasProtection"><c>null</c> to keep the existing value, or any other value to update <see cref="HasProtection"/>.</param>
        /// <param name="killCount"><c>null</c> to keep the existing value, or any other value to update <see cref="KillCount"/>.</param>
        /// <returns>The new <see cref="AssassinationTable"/>.</returns>
        public AssassinationTable With(bool? hasProtection = null, int? killCount = null)
        {
            return new AssassinationTable(
                hasProtection ?? this.hasProtection,
                killCount ?? this.killCount);
        }
    }
}
