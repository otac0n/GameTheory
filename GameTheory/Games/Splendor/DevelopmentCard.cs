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
        /// <param name="bonus">The bonus granted by this card.</param>
        /// <param name="cost">The cost of this card.</param>
        public DevelopmentCard(int prestige, Token bonus, EnumCollection<Token> cost)
        {
            this.Prestige = prestige;
            this.Cost = cost;
            this.Bonus = bonus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentCard"/> class.
        /// </summary>
        /// <param name="prestige">The prestige awarded by this card.</param>
        /// <param name="bonus">The bonus granted by this card.</param>
        /// <param name="diamond">The diamond component of the cost.</param>
        /// <param name="sapphire">The sapphire component of the cost.</param>
        /// <param name="emerald">The emerald component of the cost.</param>
        /// <param name="ruby">The ruby component of the cost.</param>
        /// <param name="onyx">The onyx component of the cost.</param>
        public DevelopmentCard(int prestige, Token bonus, int diamond = 0, int sapphire = 0, int emerald = 0, int ruby = 0, int onyx = 0)
            : this(prestige, bonus, EnumCollection<Token>.Empty
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

        /// <summary>
        /// Gets the prestige awarded by this card.
        /// </summary>
        public int Prestige { get; }

        /// <inheritdoc />
        public override string ToString() => this.Bonus.ToString() + (this.Prestige > 0 ? $" +{this.Prestige}" : string.Empty);
    }
}
