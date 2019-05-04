// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;

    public class BuiltInType : ExpressionType
    {
        public BuiltInType(Type type)
        {
            this.Type = type;
        }

        public Type Type { get; }

        /// <inheritdoc />
        public override string ToString() => this.Type.ToString();
    }
}
