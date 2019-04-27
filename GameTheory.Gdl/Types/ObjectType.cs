// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using KnowledgeInterchangeFormat.Expressions;

    /// <summary>
    /// The root type all other fully-constructed types inherit from.
    /// </summary>
    public class ObjectType : ExpressionType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectType"/> class.
        /// </summary>
        /// <param name="constant">The constant corresponding to this type.</param>
        public ObjectType(Constant constant)
        {
            this.Constant = constant;
            this.BuiltInType = typeof(string);
        }

        public Constant Constant { get; }

        /// <inheritdoc/>
        public override string ToString() => this.Constant.Id;
    }
}
