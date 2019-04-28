// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KnowledgeInterchangeFormat.Expressions;

    public class RelationInfo : ExpressionWithArgumentsInfo
    {
        public RelationInfo(Constant constant, int arity, IEnumerable<Sentence> body)
            : base(constant, arity, BooleanType.Instance)
        {
            this.Body = body.ToList();
        }

        public override ExpressionType ReturnType
        {
            get => base.ReturnType;
            set => throw new InvalidOperationException();
        }

        public List<Sentence> Body { get; }
    }
}
