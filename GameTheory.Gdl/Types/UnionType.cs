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

        public override Type BuiltInType
        {
            get
            {
                return this.Expressions
                    .Select(e => e.ReturnType.BuiltInType ?? typeof(object))
                    .Aggregate((a, b) =>
                    {
                        if (a.IsAssignableFrom(b))
                        {
                            return a;
                        }
                        else if (b.IsAssignableFrom(a))
                        {
                            return b;
                        }
                        else
                        {
                            return typeof(object);
                        }
                    });
            }

            protected set
            {
                throw new InvalidOperationException();
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"(any-of {string.Join(" or ", this.Expressions)})";
    }
}
