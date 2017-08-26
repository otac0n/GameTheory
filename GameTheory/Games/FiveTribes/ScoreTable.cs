// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.FiveTribes
{
    using System;

    /// <summary>
    /// Represents the current state of a player's score table.
    /// </summary>
    public class ScoreTable : IComparable<ScoreTable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreTable"/> class.
        /// </summary>
        public ScoreTable()
        {
            this.BuilderMultiplier = 1;
            this.ElderValue = 2;
            this.PalaceValue = 5;
            this.PalmTreeValue = 3;
            this.VizierValue = 1;
        }

        private ScoreTable(int builderMultiplier, int elderValue, int palaceValue, int palmTreeValue, int vizierValue)
        {
            this.BuilderMultiplier = builderMultiplier;
            this.ElderValue = elderValue;
            this.PalaceValue = palaceValue;
            this.PalmTreeValue = palmTreeValue;
            this.VizierValue = vizierValue;
        }

        /// <summary>
        /// Gets the player's Gold Coin (GC) multiplier applied when scoring <see cref="Meeple.Builder">Builders</see>.
        /// </summary>
        public int BuilderMultiplier { get; }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for <see cref="Meeple.Elder">Elders</see>.
        /// </summary>
        public int ElderValue { get; }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for Palaces.
        /// </summary>
        public int PalaceValue { get; }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for Palm Trees.
        /// </summary>
        public int PalmTreeValue { get; }

        /// <summary>
        /// Gets the player's base Victory Point (VP) value for <see cref="Meeple.Vizier">Viziers</see>.
        /// </summary>
        public int VizierValue { get; }

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
                builderMultiplier ?? this.BuilderMultiplier,
                elderValue ?? this.ElderValue,
                palaceValue ?? this.PalaceValue,
                palmTreeValue ?? this.PalmTreeValue,
                vizierValue ?? this.VizierValue);
        }

        /// <inheritdoc/>
        public int CompareTo(ScoreTable other)
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

            if ((comp = this.BuilderMultiplier.CompareTo(other.BuilderMultiplier)) != 0 ||
                (comp = this.ElderValue.CompareTo(other.ElderValue)) != 0 ||
                (comp = this.PalaceValue.CompareTo(other.PalaceValue)) != 0 ||
                (comp = this.PalmTreeValue.CompareTo(other.PalmTreeValue)) != 0 ||
                (comp = this.VizierValue.CompareTo(other.VizierValue)) != 0)
            {
                return comp;
            }

            return 0;
        }
    }
}
