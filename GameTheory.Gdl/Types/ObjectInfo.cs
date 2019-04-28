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
                this.ReturnType = NumberRangeType.GetInstance(value, value);
                this.Value = value;
            }
            else
            {
                this.ReturnType = new ObjectType(this);
                this.Value = constant.Id;
            }
        }

        public object Value { get; set; }
    }
}
