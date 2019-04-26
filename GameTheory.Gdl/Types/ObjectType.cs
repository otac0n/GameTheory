// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using KnowledgeInterchangeFormat.Expressions;

    /// <summary>
    /// The root type all other fully-constructed types inherit from.
    /// </summary>
    public class ObjectType : ExpressionType
    {
        private readonly ExpressionType baseType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectType"/> class.
        /// </summary>
        /// <param name="constant">The constant corresponding to this type.</param>
        /// <param name="builtInType">The built-in type to use for this expression type.</param>
        /// <param name="baseType">The base type for this type.</param>
        public ObjectType(Constant constant, Type builtInType = null, ExpressionType baseType = null)
        {
            this.Constant = constant;
            this.baseType = baseType;
            this.BuiltInType = builtInType;
        }

        public Constant Constant { get; }

        /// <inheritdoc/>
        public override ExpressionType BaseType => this.baseType ?? base.BaseType;

        /// <inheritdoc/>
        public override string ToString() => this.Constant.Id;
    }
}
