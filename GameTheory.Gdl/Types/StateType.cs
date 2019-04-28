namespace GameTheory.Gdl.Types
{
    using System.Collections.Immutable;

    public class StateType : ExpressionType
    {
        public StateType()
        {
            this.Relations = ImmutableHashSet<ExpressionInfo>.Empty;
        }

        public ImmutableHashSet<ExpressionInfo> Relations { get; set; }

        public override string ToString() => "State";
    }
}
