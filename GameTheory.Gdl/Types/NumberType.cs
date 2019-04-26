// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    /// <summary>
    /// The type shared by all decimal numbers.
    /// </summary>
    public class NumberType : ExpressionType
    {
        /// <summary>
        /// The type shared by all decimal numbers, <c>number</c>.
        /// </summary>
        /// <remarks>
        /// Underlying type is <see cref="int"/>.
        /// </remarks>
        public static readonly NumberType Instance = new NumberType();

        private NumberType()
        {
            this.BuiltInType = typeof(int);
        }

        /// <inheritdoc />
        public override string ToString() => "number";
    }
}
