// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;

    public class RelationInfo : ExpressionWithArgumentsInfo
    {
        public RelationInfo(string id, int arity)
            : base(id, arity, BooleanType.Instance)
        {
        }

        public override ExpressionType ReturnType
        {
            get => base.ReturnType;
            set => throw new InvalidOperationException();
        }
    }
}
