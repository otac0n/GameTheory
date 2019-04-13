// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections.Immutable;

    public class IntersectionType : ExpressionType
    {
        public IntersectionType()
            : base(null)
        {
            this.Expressions = ImmutableHashSet<ExpressionInfo>.Empty;
        }

        public ImmutableHashSet<ExpressionInfo> Expressions { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"(is-all {string.Join(" and ", this.Expressions)})";
    }
}
