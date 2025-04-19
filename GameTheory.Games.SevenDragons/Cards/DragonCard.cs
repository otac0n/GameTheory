// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a dragon card.
    /// </summary>
    public sealed class DragonCard : Card
    {
        /// <summary>
        /// The dimensions of the color grid in each card.
        /// </summary>
        public static readonly Size Grid = new Size(2, 2);

        /// <summary>
        /// Initializes a new instance of the <see cref="DragonCard"/> class.
        /// </summary>
        /// <param name="color">The solid color for the whole card.</param>
        public DragonCard(Color color)
            : this(Enumerable.Range(0, Grid.Count).Select(i => color).ToImmutableList(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DragonCard"/> class.
        /// </summary>
        /// <param name="colors">The colors of the card.</param>
        /// <remarks>
        /// The order of the <see cref="Color"/> components is required to match that of the see <see cref="Grid"/>.
        /// </remarks>
        public DragonCard(params Color[] colors)
            : this(Validate(colors), null)
        {
        }

        private DragonCard(ImmutableList<Color> colors, DragonCard reversed)
        {
            this.Colors = colors;
            this.Reversed = reversed ?? (IsSymmetrical(colors) ? this : new DragonCard(Reverse(colors), this));
        }

        /// <summary>
        /// Gets the colors on this card.
        /// </summary>
        public ImmutableList<Color> Colors { get; }

        /// <summary>
        /// Gets the reversed form of this card.
        /// </summary>
        public DragonCard Reversed { get; }

        /// <inheritdoc />
        public override int CompareTo(Card other)
        {
            if (other is DragonCard dragonCard)
            {
                return CompareUtilities.CompareEnumLists(this.Colors, dragonCard.Colors);
            }
            else
            {
                return base.CompareTo(other);
            }
        }

        private static bool IsSymmetrical(ImmutableList<Color> colors)
        {
            for (var i = 0; i < Grid.Count; i++)
            {
                var point = Grid[i];
                var other = Reverse(point);
                if (colors[i] != colors[Grid.IndexOf(other)])
                {
                    return false;
                }
            }

            return true;
        }

        private static Point Reverse(Point point) => new Point((Grid.Width - 1) - point.X, (Grid.Height - 1) - point.Y);

        private static ImmutableList<Color> Reverse(IList<Color> colors)
        {
            var builder = ImmutableList.CreateBuilder<Color>();

            for (var i = 0; i < Grid.Count; i++)
            {
                var point = Grid[i];
                var other = Reverse(point);
                builder.Add(colors[Grid.IndexOf(other)]);
            }

            return builder.ToImmutable();
        }

        private static ImmutableList<Color> Validate(IList<Color> colors)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(colors.Count, Grid.Count, nameof(colors));

            return colors as ImmutableList<Color> ?? colors.ToImmutableList();
        }
    }
}
