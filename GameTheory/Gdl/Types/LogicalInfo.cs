// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;

    public class LogicalInfo : ExpressionInfo
    {
        public LogicalInfo(string id)
            : base(id, BooleanType.Instance)
        {
        }

        public override ExpressionType ReturnType
        {
            get => base.ReturnType;
            set => throw new InvalidOperationException();
        }
    }
}
