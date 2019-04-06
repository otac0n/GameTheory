namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ArgumentInfo : VariableInfo
    {
        public ArgumentInfo(int index)
            : base("#" + index)
        {
            this.ReturnType = new UnionType();
        }
    }
}
