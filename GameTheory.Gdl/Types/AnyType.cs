namespace GameTheory.Gdl.Types
{
    public class AnyType : ExpressionType
    {
        public static readonly AnyType Instance = new AnyType();

        private AnyType()
        {
            this.BuiltInType = typeof(object);
        }

        /// <inheritdoc />
        public override string ToString() => "(any)";
    }
}
