// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public class VariableInfo : ExpressionInfo
    {
        public VariableInfo(string id)
            : base(id, new IntersectionType())
        {
        }
    }
}
