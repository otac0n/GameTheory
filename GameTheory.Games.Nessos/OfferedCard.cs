// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos
{
    using System;

    /// <summary>
    /// Represents the card or cards being offered from one player to another.
    /// </summary>
    public sealed class OfferedCard : IComparable<OfferedCard>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OfferedCard"/> class.
        /// </summary>
        /// <param name="actualCard">The card being offered.</param>
        /// <param name="claimedCard">The card being claimed.</param>
        /// <param name="sourcePlayer">They player offering a new card.</param>
        public OfferedCard(Card actualCard, Card claimedCard, PlayerToken sourcePlayer)
        {
            this.ActualCard = actualCard;
            this.ClaimedCard = claimedCard;
            this.SourcePlayer = sourcePlayer;
        }

        /// <summary>
        /// Get the card being offered.
        /// </summary>
        public Card ActualCard { get; }

        /// <summary>
        /// Get the card being claimed.
        /// </summary>
        public Card ClaimedCard { get; }

        /// <summary>
        /// Gets the player offering a new card.
        /// </summary>
        public PlayerToken SourcePlayer { get; }

        public int CompareTo(OfferedCard other)
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

            if ((comp = this.SourcePlayer.CompareTo(other.SourcePlayer)) != 0 ||
                (comp = this.ActualCard.CompareTo(other.ActualCard)) != 0 ||
                (comp = this.ClaimedCard.CompareTo(other.ClaimedCard)) != 0)
            {
                return comp;
            }

            return 0;
        }
    }
}
