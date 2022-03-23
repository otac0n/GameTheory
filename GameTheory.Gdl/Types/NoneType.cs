// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public sealed class NoneType : ExpressionType
    {
        public static readonly NoneType Instance = new NoneType();

        private NoneType()
        {
        }

        /// <inheritdoc />
        public override ExpressionType BaseType => this;

        /// <inheritdoc />
        public override string ToString() => "(none)";
    }
}
