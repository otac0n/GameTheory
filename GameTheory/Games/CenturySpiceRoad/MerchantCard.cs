// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;

    /// <summary>
    /// Represents a Merchant card in Century Spice Road.
    /// </summary>
    public abstract class MerchantCard : IComparable<MerchantCard>
    {
        /// <inheritdoc/>
        public virtual int CompareTo(MerchantCard other)
        {
            return this.GetType().Name.CompareTo(other.GetType().Name);
        }

        /// <inheritdoc/>
        public abstract override string ToString();
    }
}
