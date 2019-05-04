// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    /// <summary>
    /// Describes types in the expression types hierarchy.
    /// </summary>
    public abstract class ExpressionType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionType"/> class.
        /// </summary>
        public ExpressionType()
        {
        }

        /// <summary>
        /// Gets the storage type to use for this type.
        /// </summary>
        public virtual ExpressionType StorageType => this;

        /// <summary>
        /// Gets the base type for this type.
        /// </summary>
        /// <remarks>
        /// All objects ultimately inherit from <see cref="AnyType.Instance"/>.
        /// The <see cref="NoneType"><c>none</c></see> type doesn't represent any objects, so it is its own base class.
        /// </remarks>
        public virtual ExpressionType BaseType => AnyType.Instance;

        /// <inheritdoc/>
        public abstract override string ToString();
    }
}
