namespace GameTheory.Gdl.Types
{
    public class NoneType : ExpressionType
    {
        public static readonly NoneType Instance = new NoneType();

        private NoneType()
            : base("(none)")
        {
        }

        /// <inheritdoc />
        public override ExpressionType BaseType => this;
    }
}
