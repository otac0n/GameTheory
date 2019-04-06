// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class FunctionInfo : ExpressionInfo
    {
        private readonly ExpressionType[] argumentTypes;

        public FunctionInfo(string id, int arity)
            : base(id, arity)
        {
            this.argumentTypes = new ExpressionType[arity];
        }

        public override ExpressionType[] ArgumentTypes => this.argumentTypes;
    }
}
