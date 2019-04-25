// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using KnowledgeInterchangeFormat.Expressions;

    public class ExpressionWithArgumentsInfo : ConstantInfo
    {
        public ExpressionWithArgumentsInfo(Constant constant, int arity, ExpressionType returnType)
            : base(constant, returnType)
        {
            this.Arity = arity;
            this.Arguments = new ArgumentInfo[arity];
            for (var i = 0; i < arity; i++)
            {
                this.Arguments[i] = new ArgumentInfo(this, i);
            }
        }

        public int Arity { get; }

        public virtual ArgumentInfo[] Arguments { get; protected set; }

        public virtual Scope<VariableInfo> Scope { get; set; }

        public override string ToString() => $"{base.ToString()}_{this.Arity}";
    }
}
