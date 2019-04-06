namespace GameTheory.Gdl.Types
{
    using System.Collections.Generic;

    public class IntersectionType : ExpressionType
    {
        public IntersectionType()
        {
            this.Expressions = new HashSet<ExpressionInfo>();
        }

        public HashSet<ExpressionInfo> Expressions { get; }

        /// <inheritdoc/>
        public override string ToString() => string.Join(" & ", this.Expressions);
    }
}
