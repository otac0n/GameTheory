// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.CenturySpiceRoad
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a Merchant card in Century Spice Road.
    /// </summary>
    public abstract class MerchantCard : IComparable<MerchantCard>, ITokenFormattable
    {
        /// <inheritdoc/>
        public abstract IList<object> FormatTokens { get; }

        /// <inheritdoc/>
        public virtual int CompareTo(MerchantCard other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return 1;
            }

            return string.Compare(this.GetType().FullName, other.GetType().FullName, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        public sealed override string ToString() => string.Concat(this.FlattenFormatTokens());
    }
}
