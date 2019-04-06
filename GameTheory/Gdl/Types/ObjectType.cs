// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ObjectType : ExpressionType
    {
        public static readonly ObjectType Instance = new ObjectType("object");

        protected ObjectType(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        /// <inheritdoc/>
        public override string ToString() => this.Name;
    }
}
