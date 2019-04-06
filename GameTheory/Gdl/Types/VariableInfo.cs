namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class VariableInfo : ExpressionInfo
    {
        public VariableInfo(string id)
            : base(id)
        {
            this.ReturnType = new IntersectionType();
        }
    }
}
