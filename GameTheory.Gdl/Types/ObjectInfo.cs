// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public class ObjectInfo : ExpressionInfo
    {
        public ObjectInfo(string id, ExpressionType returnType, object value = null)
            : base(id, returnType)
        {
            this.Value = value;
        }

        public object Value { get; set; }
    }
}
