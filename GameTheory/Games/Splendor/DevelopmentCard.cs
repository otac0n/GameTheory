// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Splendor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The initial development deck the development card appeared in.
    /// </summary>
    public enum DevelopmentLevel : byte
    {
        /// <summary>
        /// The first development deck.
        /// </summary>
        Level1 = 0,

        /// <summary>
        /// The second development deck.
        /// </summary>
        Level2 = 1,

        /// <summary>
        /// The third development deck.
        /// </summary>
        Level3 = 2,
    }

    /// <summary>
    /// Describes a development card.
    /// </summary>
    public sealed class DevelopmentCard : IComparable<DevelopmentCard>, ITokenFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentCard"/> class.
        /// </summary>
        /// <param name="level">The initial development deck the development card appeared in.</param>
        /// <param name="prestige">The prestige awarded by this card.</param>
        /// <param name="bonus">The bonus granted by this card.</param>
        /// <param name="cost">The cost of this card.</param>
        public DevelopmentCard(DevelopmentLevel level, int prestige, Token bonus, EnumCollection<Token> cost)
        {
            this.Level = level;
            this.Prestige = prestige;
            this.Cost = cost;
            this.Bonus = bonus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentCard"/> class.
        /// </summary>
        /// <param name="level">The initial development deck the development card appeared in.</param>
        /// <param name="prestige">The prestige awarded by this card.</param>
        /// <param name="bonus">The bonus granted by this card.</param>
        /// <param name="diamond">The diamond component of the cost.</param>
        /// <param name="sapphire">The sapphire component of the cost.</param>
        /// <param name="emerald">The emerald component of the cost.</param>
        /// <param name="ruby">The ruby component of the cost.</param>
        /// <param name="onyx">The onyx component of the cost.</param>
        public DevelopmentCard(DevelopmentLevel level, int prestige, Token bonus, int diamond = 0, int sapphire = 0, int emerald = 0, int ruby = 0, int onyx = 0)
            : this(level, prestige, bonus, EnumCollection<Token>.Empty
                  .Add(Token.Diamond, diamond)
                  .Add(Token.Sapphire, sapphire)
                  .Add(Token.Emerald, emerald)
                  .Add(Token.Ruby, ruby)
                  .Add(Token.Onyx, onyx))
        {
        }

        /// <summary>
        /// Gets the bonus granted by this card.
        /// </summary>
        public Token Bonus { get; }

        /// <summary>
        /// Gets the cost of this card.
        /// </summary>
        public EnumCollection<Token> Cost { get; }

        /// <inheritdoc />
        public IList<object> FormatTokens => this.Prestige > 0
            ? new object[] { this.Bonus, " +", this.Prestige }
            : new object[] { this.Bonus };

        /// <summary>
        /// Gets the initial development deck the development card appeared in.
        /// </summary>
        public DevelopmentLevel Level { get; }

        /// <summary>
        /// Gets the prestige awarded by this card.
        /// </summary>
        public int Prestige { get; }

        /// <inheritdoc/>
        public int CompareTo(DevelopmentCard other)
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
            if ((comp = this.Prestige.CompareTo(other.Prestige)) != 0 ||
                (comp = this.Cost.CompareTo(other.Cost)) != 0 ||
                (comp = this.Bonus.CompareTo(other.Bonus)) != 0)
            {
                return comp;
            }

            return 0;
        }

        /// <inheritdoc />
        public override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
