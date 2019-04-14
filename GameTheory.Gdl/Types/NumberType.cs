// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    /// <summary>
    /// The type shared by all decimal numbers.
    /// </summary>
    public class NumberType : ObjectType
    {
        /// <summary>
        /// The type shared by all decimal numbers, <c>number</c>.
        /// </summary>
        /// <remarks>
        /// Underlying type is <see cref="int"/>.
        /// </remarks>
        public static new readonly NumberType Instance = new NumberType();

        private NumberType()
            : base("number", builtInType: typeof(int))
        {
        }
    }
}
