namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ExpressionWithArgumentsInfo : ExpressionInfo
    {
        public ExpressionWithArgumentsInfo(string id, int arity)
            : base(id)
        {
            this.Arity = arity;
            this.Arguments = new ArgumentInfo[arity];
            for (var i = 0; i < arity; i++)
            {
                this.Arguments[i] = new ArgumentInfo(i);
            }
        }

        public int Arity { get; }

        public virtual ArgumentInfo[] Arguments { get; protected set; }

        /// <inheritdoc/>
        public override string ToString() => $"{this.Id}{(this.Arity > 0 ? $"@{this.Arity}" : null)}";
    }
}
