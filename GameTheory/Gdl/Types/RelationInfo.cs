// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;

    public class RelationInfo : ExpressionWithArgumentsInfo
    {
        private readonly ArgumentInfo[] argumentTypes;

        public RelationInfo(string id, int arity)
            : base(id, arity)
        {
        }

        public override ExpressionType ReturnType
        {
            get => BooleanType.Instance;
            set => throw new InvalidOperationException();
        }
    }
}
