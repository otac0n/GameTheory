// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    public class UnionType : ExpressionType
    {
        public UnionType()
        {
            this.Expressions = ImmutableHashSet<ExpressionInfo>.Empty;
        }

        public ImmutableHashSet<ExpressionInfo> Expressions { get; set; }

        public override ExpressionType StorageType =>
            this.Expressions
                .Select(e => e.ReturnType.StorageType)
                .Aggregate((a, b) =>
                {
                    if (a == b || a is AnyType || b is NoneType)
                    {
                        return a;
                    }
                    else if (b is AnyType || a is NoneType)
                    {
                        return b;
                    }

                    if (a is NumberRangeType aRange && b is NumberRangeType bRange)
                    {
                        return NumberRangeType.GetInstance(Math.Min(aRange.Start, bRange.Start), Math.Max(aRange.End, bRange.End));
                    }

                    return AnyType.Instance;
                });

        /// <inheritdoc/>
        public override string ToString() => $"(any-of {string.Join(" or ", this.Expressions)})";
    }
}
