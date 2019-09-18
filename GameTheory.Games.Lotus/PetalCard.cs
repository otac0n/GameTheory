// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Lotus
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a petal card in Lotus.
    /// </summary>
    public class PetalCard : IComparable<PetalCard>, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PetalCard"/> class.
        /// </summary>
        /// <param name="flowerType">The kind of flower this petal belongs to.</param>
        /// <param name="guardians">The owner of this card, or <c>null</c> if it is a wildflower card.</param>
        /// <param name="owner">The number of guardians on this card.</param>
        public PetalCard(FlowerType flowerType, PlayerToken owner = null, int guardians = 0)
        {
            this.FlowerType = flowerType;
            this.Owner = owner;
            this.Guardians = guardians;
        }

        /// <summary>
        /// Gets an immutable wild iris.
        /// </summary>
        public static PetalCard WildCherryBlossom { get; } = new PetalCard(FlowerType.CherryBlossom);

        /// <summary>
        /// Gets an immutable wild iris.
        /// </summary>
        public static PetalCard WildIris { get; } = new PetalCard(FlowerType.Iris);

        /// <summary>
        /// Gets an immutable wild iris.
        /// </summary>
        public static PetalCard WildLily { get; } = new PetalCard(FlowerType.Lily);

        /// <summary>
        /// Gets an immutable wild iris.
        /// </summary>
        public static PetalCard WildLotus { get; } = new PetalCard(FlowerType.Lotus);

        /// <summary>
        /// Gets an immutable wild primrose.
        /// </summary>
        public static PetalCard WildPrimrose { get; } = new PetalCard(FlowerType.Primrose);

        /// <summary>
        /// Gets the kind of flower this petal belongs to.
        /// </summary>
        public FlowerType FlowerType { get; }

        /// <inheritdoc/>
        public IList<object> FormatTokens =>
            FormatUtilities.ParseStringFormat(Resources.PetalCardFormat, this.FlowerType, string.Format(Resources.GuardianBonusFormat, this.Guardians));

        /// <summary>
        /// Gets the number of guardians on this card.
        /// </summary>
        public int Guardians { get; }

        /// <summary>
        /// Gets the owner of this card, or <c>null</c> if it is a wildflower card.
        /// </summary>
        public PlayerToken Owner { get; }

        /// <inheritdoc/>
        public int CompareTo(PetalCard other)
        {
            if (object.ReferenceEquals(other, this))
            {
                return 0;
            }
            else if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int comp;

            if ((comp = EnumComparer<FlowerType>.Default.Compare(this.FlowerType, other.FlowerType)) != 0 ||
                (comp = this.Guardians.CompareTo(other.Guardians)) != 0)
            {
                return comp;
            }

            return this.Owner == null
                ? (other.Owner != null ? -1 : 0)
                : this.Owner.CompareTo(other.Owner);
        }
    }
}
