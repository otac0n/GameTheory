// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    /// <summary>
    /// The root type all other fully-constructed types inherit from.
    /// </summary>
    public class ObjectType : ExpressionType
    {
        private static readonly BuiltInType ObjectStorageType = new BuiltInType(typeof(string));

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectType"/> class.
        /// </summary>
        /// <param name="objectInfo">The <see cref="ObjectInfo"/> corresponding to this type.</param>
        public ObjectType(ObjectInfo objectInfo)
        {
            this.ObjectInfo = objectInfo;
        }

        public override ExpressionType StorageType => ObjectStorageType;

        public ObjectInfo ObjectInfo { get; }

        /// <inheritdoc/>
        public override string ToString() => this.ObjectInfo.Constant.Id;
    }
}
