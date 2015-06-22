// -----------------------------------------------------------------------
// <copyright file="ScoreTable.cs" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    /// <summary>
    /// Represents the current state of a player's score table.
    /// </summary>
    public class ScoreTable
    {
        private readonly int builderMultiplier;
        private readonly int elderValue;
        private readonly int palaceValue;
        private readonly int palmTreeValue;
        private readonly int vizierValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreTable"/> class.
        /// </summary>
        public ScoreTable()
        {
            this.builderMultiplier = 1;
            this.elderValue = 2;
            this.palaceValue = 5;
            this.palmTreeValue = 3;
            this.vizierValue = 1;
        }

        private ScoreTable(int builderMultiplier, int elderValue, int palaceValue, int palmTreeValue, int vizierValue)
        {
            this.builderMultiplier = builderMultiplier;
            this.elderValue = elderValue;
            this.palaceValue = palaceValue;
            this.palmTreeValue = palmTreeValue;
            this.vizierValue = vizierValue;
        }

        /// <summary>
        /// Gets the player's Gold Coin (GC) multiplier applied when scoring <see cref="Meeple.Builder">Builders</see>.
        /// </summary>
        public int BuilderMultiplier
        {
            get { return this.builderMultiplier; }
        }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for <see cref="Meeple.Elder">Elders</see>.
        /// </summary>
        public int ElderValue
        {
            get { return this.elderValue; }
        }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for Palaces.
        /// </summary>
        public int PalaceValue
        {
            get { return this.palaceValue; }
        }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for Palm Trees.
        /// </summary>
        public int PalmTreeValue
        {
            get { return this.palmTreeValue; }
        }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for <see cref="Meeple.Vizier">Viziers</see>.
        /// </summary>
        public int VizierValue
        {
            get { return this.vizierValue; }
        }

        /// <summary>
        /// Creates a new <see cref="ScoreTable"/>, and updates the specified values.
        /// </summary>
        /// <param name="builderMultiplier"><c>null</c> to keep the existing value, or any other value to update <see cref="BuilderMultiplier"/>.</param>
        /// <param name="elderValue"><c>null</c> to keep the existing value, or any other value to update <see cref="ElderValue"/>.</param>
        /// <param name="palaceValue"><c>null</c> to keep the existing value, or any other value to update <see cref="PalaceValue"/>.</param>
        /// <param name="palmTreeValue"><c>null</c> to keep the existing value, or any other value to update <see cref="PalmTreeValue"/>.</param>
        /// <param name="vizierValue"><c>null</c> to keep the existing value, or any other value to update <see cref="VizierValue"/>.</param>
        /// <returns>The new <see cref="ScoreTable"/>.</returns>
        public ScoreTable With(int? builderMultiplier = null, int? elderValue = null, int? palaceValue = null, int? palmTreeValue = null, int? vizierValue = null)
        {
            return new ScoreTable(
                builderMultiplier ?? this.builderMultiplier,
                elderValue ?? this.elderValue,
                palaceValue ?? this.palaceValue,
                palmTreeValue ?? this.palmTreeValue,
                vizierValue ?? this.vizierValue);
        }
    }
}
