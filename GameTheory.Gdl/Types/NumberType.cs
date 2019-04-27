// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using KnowledgeInterchangeFormat.Expressions;

    /// <summary>
    /// The type shared by all decimal numbers.
    /// </summary>
    public class NumberType : ExpressionType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberType"/> class.
        /// </summary>
        /// <param name="constant">The constant corresponding to this type.</param>
        public NumberType(Constant constant)
        {
            this.Constant = constant;
            this.BuiltInType = typeof(int);
        }

        public Constant Constant { get; }

        /// <inheritdoc />
        public override string ToString() => this.Constant.Id;
    }
}
