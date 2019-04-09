namespace GameTheory.Gdl.Types
{
    using System.Collections.Immutable;

    public class StructType : ExpressionType
    {
        public StructType(string name)
        {
            this.Name = name;
            this.Objects = ImmutableHashSet<ExpressionInfo>.Empty;
        }

        public string Name { get; }

        public ImmutableHashSet<ExpressionInfo> Objects { get; set; }

        public override string ToString() => this.Name;
    }
}
