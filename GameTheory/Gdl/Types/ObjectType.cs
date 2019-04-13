// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;

    /// <summary>
    /// The root type all other fully-constructed types inherit from.
    /// </summary>
    public class ObjectType : ExpressionType
    {
        /// <summary>
        /// The root type in the hierarchy, <c>object</c>.
        /// </summary>
        public static readonly ObjectType Instance = new ObjectType("object", null, typeof(object));

        private readonly ExpressionType baseType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectType"/> class.
        /// </summary>
        /// <param name="name">The name of this type.</param>
        /// <param name="baseType">The base type for this type.</param>
        /// <param name="builtInType">The built-in type to use for this expression type.</param>
        public ObjectType(string name, ExpressionType baseType = null, Type builtInType = null)
            : base(name)
        {
            this.baseType = baseType;
            this.BuiltInType = builtInType;
        }

        /// <inheritdoc/>
        public override ExpressionType BaseType => this.baseType ?? base.BaseType;
    }
}
