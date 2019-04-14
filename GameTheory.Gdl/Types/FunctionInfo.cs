// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;

    public class FunctionInfo : ExpressionWithArgumentsInfo
    {
        public FunctionInfo(string id, int arity)
            : base(id, arity, null)
        {
            base.ReturnType = new FunctionType(this);
        }

        public override ExpressionType ReturnType
        {
            get => base.ReturnType;
            set => throw new InvalidOperationException();
        }
    }
}
