// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public class ObjectType : ExpressionType
    {
        public static readonly ObjectType Instance = new ObjectType("object");

        public ObjectType(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        /// <inheritdoc/>
        public override string ToString() => this.Name;
    }
}
