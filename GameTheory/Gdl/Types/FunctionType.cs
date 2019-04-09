namespace GameTheory.Gdl.Types
{
    public class FunctionType : ObjectType
    {
        public FunctionType(FunctionInfo functionInfo)
            : base($"{functionInfo.Id}_{functionInfo.Arity}")
        {
            this.FunctionInfo = functionInfo;
        }

        public FunctionInfo FunctionInfo { get; }
    }
}
