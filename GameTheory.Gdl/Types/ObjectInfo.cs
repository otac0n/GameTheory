// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using KnowledgeInterchangeFormat.Expressions;

    public class ObjectInfo : ConstantInfo
    {
        public ObjectInfo(Constant constant, ExpressionType returnType, object value = null)
            : base(constant, returnType)
        {
            this.Value = value;
        }

        public object Value { get; set; }
    }
}
