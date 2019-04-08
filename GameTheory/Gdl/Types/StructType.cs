namespace GameTheory.Gdl.Types
{
    using System.Collections.Generic;

    public class StructType : ExpressionType
    {
        public StructType(string name)
        {
            this.Name = name;
            this.Objects = new HashSet<ExpressionInfo>();
        }

        public string Name { get; }

        public HashSet<ExpressionInfo> Objects { get; }

        public override string ToString() => this.Name;
    }
}
