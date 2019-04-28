namespace GameTheory.Gdl.Types
{
    public class NoneType : ExpressionType
    {
        public static readonly NoneType Instance = new NoneType();

        private NoneType()
        {
            this.BuiltInType = typeof(void);
        }

        /// <inheritdoc />
        public override ExpressionType BaseType => this;

        /// <inheritdoc />
        public override string ToString() => "(none)";
    }
}
