// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections.Generic;

    public class EnumType : ExpressionType
    {
        private EnumType(string name)
        {
            this.Name = name;
            this.Objects = new HashSet<ObjectInfo>();
        }

        public string Name { get; }

        public HashSet<ObjectInfo> Objects { get; }

        public static EnumType Create(string name, IEnumerable<ObjectInfo> objects)
        {
            var enumType = new EnumType(name);

            foreach (var obj in objects)
            {
                enumType.Objects.Add(obj);
                obj.ReturnType = enumType;
            }

            return enumType;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{this.Name}{{{string.Join(", ", this.Objects)}}}";
    }
}
