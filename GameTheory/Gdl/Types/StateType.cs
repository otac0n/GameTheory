namespace GameTheory.Gdl.Types
{
    using System.Collections.Immutable;

    public class StateType : ExpressionType
    {
        public StateType(string name)
        {
            this.Name = name;
            this.Relations = ImmutableHashSet<ExpressionInfo>.Empty;
        }

        public string Name { get; }

        public ImmutableHashSet<ExpressionInfo> Relations { get; set; }

        public override string ToString() => this.Name;
    }
}
