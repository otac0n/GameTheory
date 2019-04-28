namespace GameTheory.Gdl.Types
{
    using KnowledgeInterchangeFormat.Expressions;

    public abstract class ConstantInfo : ExpressionInfo
    {
        public ConstantInfo(Constant constant, ExpressionType returnType)
            : base(constant.Id, returnType)
        {
            this.Constant = constant;
        }

        public Constant Constant { get; }

        public override string ToString() => this.Constant.Name;
    }
}
