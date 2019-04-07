// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public class FunctionInfo : ExpressionWithArgumentsInfo
    {
        private readonly ArgumentInfo[] argumentTypes;

        public FunctionInfo(string id, int arity)
            : base(id, arity)
        {
        }
    }
}
