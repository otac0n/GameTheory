// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class EnumType : ExpressionType
    {
        private EnumType(RelationInfo relationInfo, IEnumerable<ObjectInfo> objects)
        {
            this.RelationInfo = relationInfo;
            this.Objects = ImmutableHashSet.CreateRange(objects);
            this.Scope = this.Objects.Aggregate(new Scope<ObjectInfo>(), (scope, o) => scope.Add(o, ScopeFlags.Public, o.Constant.Name));
        }

        public RelationInfo RelationInfo { get; }

        public ImmutableHashSet<ObjectInfo> Objects { get; }

        public Scope<ObjectInfo> Scope { get; }

        public static EnumType Create(RelationInfo relationInfo, IEnumerable<ObjectInfo> objects)
        {
            var enumType = new EnumType(relationInfo, objects);

            foreach (var obj in enumType.Objects)
            {
                obj.ReturnType = enumType;
            }

            return enumType;
        }

        public override string ToString() => this.RelationInfo.Id;
    }
}
