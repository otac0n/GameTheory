namespace GameTheory.Gdl.Types
{
    using System.Collections.Generic;

    public class StructType : ExpressionType
    {
        public StructType(string id)
        {
            this.Id = id;
            this.Objects = new HashSet<ExpressionInfo>();
        }

        public string Id { get; }

        public HashSet<ExpressionInfo> Objects { get; }

        public override string ToString() => $"{this.Id}{{" + string.Join(", ", this.Objects) + "}";
    }
}
