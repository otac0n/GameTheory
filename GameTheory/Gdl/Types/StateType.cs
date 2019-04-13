namespace GameTheory.Gdl.Types
{
    using System.Collections.Immutable;

    public class StateType : ExpressionType
    {
        public StateType(string name)
            : base(name)
        {
            this.Relations = ImmutableHashSet<ExpressionInfo>.Empty;
        }

        public ImmutableHashSet<ExpressionInfo> Relations { get; set; }
    }
}
