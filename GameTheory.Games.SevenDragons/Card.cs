// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons
{
    using System;

    /// <summary>
    /// Represents a card in the game of <see cref="GameState">Seven Dragons</see>.
    /// </summary>
    public abstract class Card : IComparable<Card>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Card"/> class.
        /// </summary>
        protected Card()
        {
        }

        /// <inheritdoc/>
        public virtual int CompareTo(Card other)
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

            if ((comp = string.Compare(this.GetType().FullName, other.GetType().FullName, StringComparison.Ordinal)) != 0)
            {
                return comp;
            }

            return 0;
        }
    }
}
