// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KnowledgeInterchangeFormat.Expressions;

    public class LogicalInfo : ConstantInfo
    {
        public LogicalInfo(Constant constant, IEnumerable<Sentence> body)
            : base(constant, BooleanType.Instance)
        {
            this.Body = body.ToList();
        }

        public override ExpressionType ReturnType
        {
            get => base.ReturnType;
            set => throw new InvalidOperationException();
        }

        public List<Sentence> Body { get; }

        public virtual Scope<object> Scope { get; set; }
    }
}
