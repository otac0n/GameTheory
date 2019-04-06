namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class UnboundType : ExpressionType
    {
        public UnboundType(string id)
        {
            this.Id = id;
        }

        public string Id { get; }

        public override string ToString() => $"none <= {this.Id} <= any";
    }
}
