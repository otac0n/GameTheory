// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class EnumType : ExpressionType
    {
        private EnumType(string name, IEnumerable<ObjectInfo> objects)
            : base(name)
        {
            this.Objects = ImmutableHashSet.CreateRange(objects);
        }

        public ImmutableHashSet<ObjectInfo> Objects { get; }

        public static EnumType Create(string name, IEnumerable<ObjectInfo> objects)
        {
            var enumType = new EnumType(name, objects);

            foreach (var obj in enumType.Objects)
            {
                obj.ReturnType = enumType;
            }

            return enumType;
        }
    }
}
