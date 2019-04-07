namespace GameTheory.Gdl.Types
{
    public class VariableInfo : ExpressionInfo
    {
        public VariableInfo(string id)
            : base(id, new IntersectionType())
        {
        }
    }
}
