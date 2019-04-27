// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using KnowledgeInterchangeFormat.Expressions;

    public class ObjectInfo : ConstantInfo
    {
        public ObjectInfo(Constant constant)
            : base(constant, null)
        {
            if (int.TryParse(constant.Id, out var value) && value.ToString().Equals(constant.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.ReturnType = new NumberType(constant);
                this.Value = value;
            }
            else
            {
                this.ReturnType = new ObjectType(constant);
                this.Value = constant.Id;
            }
        }

        public object Value { get; set; }
    }
}
