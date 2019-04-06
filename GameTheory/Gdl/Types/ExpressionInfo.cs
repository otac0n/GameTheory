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
        public ExpressionInfo(string id)
        {
            this.Id = id;
        }

        public string Id { get; }

        public virtual ExpressionType ReturnType { get; set; }

        public override string ToString() => this.Id;
    }
}
