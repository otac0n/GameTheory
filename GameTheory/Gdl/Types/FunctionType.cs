namespace GameTheory.Gdl.Types
{
    public class FunctionType : ObjectType
    {
        public FunctionType(FunctionInfo functionInfo)
            : base($"{functionInfo.Id}@{functionInfo.Arity}")
        {
        }
    }
}
