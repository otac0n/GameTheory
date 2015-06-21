// -----------------------------------------------------------------------
// <copyright file="AssasinationTable.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    /// <summary>
    /// Holds state regarding the assassination process.
    /// </summary>
    public class AssasinationTable
    {
        private readonly bool hasProtection;
        private readonly int killCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinationTable" /> class.
        /// </summary>
        public AssasinationTable()
            : this(false, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssassinationTable" /> class.
        /// </summary>
        /// <param name="hasProtection">A value indicating whether or not the player's Meeples have protection from assassination.</param>
        /// <param name="killCount">A number corresponding to the number of meeples that the player can kill during an assassination.</param>
        public AssasinationTable(bool hasProtection, int killCount)
        {
            this.hasProtection = hasProtection;
            this.killCount = killCount;
        }

        /// <summary>
        /// Gets a value indicating whether or not the player's Meeples have protection from assassination.
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

        public AssasinationTable With(bool? hasProtection = null, int? killCount = null)
        {
            return new AssasinationTable(
                hasProtection ?? this.hasProtection,
                killCount ?? this.killCount);
        }
    }
}
