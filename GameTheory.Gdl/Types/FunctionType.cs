namespace GameTheory.Gdl.Types
{
    public class FunctionType : ExpressionType
    {
        public FunctionType(FunctionInfo functionInfo)
        {
            this.FunctionInfo = functionInfo;
        }

        public FunctionInfo FunctionInfo { get; }

        public override string ToString() => this.FunctionInfo.Id;
    }
}
