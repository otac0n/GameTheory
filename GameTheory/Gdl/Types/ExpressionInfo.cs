// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    public abstract class ExpressionInfo
    {
        private string id;
        private ExpressionType returnType;

        public ExpressionInfo(string id, ExpressionType returnType)
        {
            this.id = id;
            this.returnType = returnType;
        }

        public virtual string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public virtual ExpressionType ReturnType
        {
            get { return this.returnType; }
            set { this.returnType = value; }
        }

        public override string ToString() => this.Id;
    }
}
