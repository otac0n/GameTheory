namespace GameTheory.Gdl.Types
{
    public class ExpressionWithArgumentsInfo : ExpressionInfo
    {
        public ExpressionWithArgumentsInfo(string id, int arity, ExpressionType returnType)
            : base(id, returnType)
        {
            this.Arity = arity;
            this.Arguments = new ArgumentInfo[arity];
            for (var i = 0; i < arity; i++)
            {
                this.Arguments[i] = new ArgumentInfo(this, i);
            }
        }

        public int Arity { get; }

        public virtual ArgumentInfo[] Arguments { get; protected set; }

        public override string ToString() => $"{this.Id}_{this.Arity}";
    }
}
