// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a Merchant card in Century Spice Road.
    /// </summary>
    public abstract class MerchantCard : IComparable<MerchantCard>
    {
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The inheriting class must override this method and provide a more specific implementation.")]
        public virtual int CompareTo(MerchantCard other)
        {
            return string.Compare(this.GetType().Name, other.GetType().Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public abstract override string ToString();
    }
}
