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
        public FunctionInfo(string id, int arity)
            : base(id, arity)
        {
            this.ArgumentTypes = new ExpressionType[arity];
        }

        public ExpressionType ReturnType { get; set; }

        public ExpressionType[] ArgumentTypes { get; }
    }
}
