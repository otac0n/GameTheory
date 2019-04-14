// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;

    /// <summary>
    /// Describes types in the expression types hierarchy.
    /// </summary>
    public abstract class ExpressionType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionType"/> class.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        public ExpressionType(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the built-in type to use for this type.
        /// </summary>
        public virtual Type BuiltInType { get; protected set; }

        /// <summary>
        /// Gets the base type for this type.
        /// </summary>
        /// <remarks>
        /// All objects ultimately inherit from <see cref="ObjectType.Instance"/>.
        /// The <see cref="NoneType"><c>none</c></see> type doesn't represent any objects, so it can be its own base class.
        /// </remarks>
        public virtual ExpressionType BaseType => ObjectType.Instance;

        /// <inheritdoc/>
        public override string ToString() => this.Name;
    }
}
