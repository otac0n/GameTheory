namespace GameTheory.Gdl.Types
{
    public class ArgumentInfo : VariableInfo
    {
        internal ArgumentInfo(ExpressionWithArgumentsInfo expression, int index)
            : base($"{expression.Id}_{index}")
        {
            this.Index = index;
            this.Expression = expression;
            this.ReturnType = new UnionType();
        }

        public int Index { get; }

        public ExpressionWithArgumentsInfo Expression { get; }
    }
}
