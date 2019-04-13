// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections.Immutable;

    public class UnionType : ExpressionType
    {
        public UnionType()
            : base(null)
        {
            this.Expressions = ImmutableHashSet<ExpressionInfo>.Empty;
        }

        public ImmutableHashSet<ExpressionInfo> Expressions { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"(any-of {string.Join(" or ", this.Expressions)})";
    }
}
