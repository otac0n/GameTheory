// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class ExpressionInfo
    {
        public ExpressionInfo(string id, int arity)
        {
            this.Id = id;
            this.Arity = arity;
        }

        public string Id { get; }

        public int Arity { get; }
    }
}
