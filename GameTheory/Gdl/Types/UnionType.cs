// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections;
    using System.Collections.Generic;

    public class UnionType : ExpressionType, IEnumerable<ExpressionInfo>
    {
        public UnionType()
        {
            this.Expressions = new HashSet<ExpressionInfo>();
        }

        public HashSet<ExpressionInfo> Expressions { get; }

        public void Add(ExpressionInfo expressionInfo)
        {
            this.Expressions.Add(expressionInfo);
        }

        /// <inheritdoc/>
        public IEnumerator<ExpressionInfo> GetEnumerator() => this.Expressions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <inheritdoc/>
        public override string ToString() => string.Join(" | ", this.Expressions);
    }
}
