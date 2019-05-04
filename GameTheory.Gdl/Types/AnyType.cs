namespace GameTheory.Gdl.Types
{
    public class AnyType : BuiltInType
    {
        public static readonly AnyType Instance = new AnyType();

        private AnyType()
            : base(typeof(object))
        {
        }

        /// <inheritdoc />
        public override string ToString() => "(any)";
    }
}
