// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections.Generic;

    public class IntersectionType : ExpressionType
    {
        public IntersectionType()
        {
            this.Expressions = new HashSet<ExpressionInfo>();
        }

        public HashSet<ExpressionInfo> Expressions { get; }

        /// <inheritdoc/>
        public override string ToString() => $"(is-all {string.Join(" and ", this.Expressions)})";
    }
}
