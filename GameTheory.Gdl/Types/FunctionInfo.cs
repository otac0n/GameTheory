// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using KnowledgeInterchangeFormat.Expressions;

    public class FunctionInfo : ExpressionWithArgumentsInfo
    {
        public FunctionInfo(Constant constant, int arity)
            : base(constant, arity, null)
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
