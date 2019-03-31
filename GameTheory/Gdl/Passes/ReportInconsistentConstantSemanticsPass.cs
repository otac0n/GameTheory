// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    internal class ReportInconsistentConstantSemanticsPass : CompilePass
    {
        public const string InconsistentConstantSemanticsError = "GDL002";

        public override IList<string> BlockedByErrors => new[]
        {
            ReportUnsupportedExpressionTypesPass.UnsupportedExpressionTypeError,
        };

        public override IList<string> ErrorsProduced => new[]
        {
            InconsistentConstantSemanticsError,
        };

        public override void Run(CompileResult result)
        {
            new ConstantSemanticsWalker(result).Walk((Expression)result.KnowledgeBase);
            foreach (var item in result.ConstantTypes.ToList())
            {
                if (item.Value == ConstantType.Unknown)
                {
                    result.ConstantTypes[item.Key] = ConstantType.Object;
                }
            }
        }

        private class ConstantSemanticsWalker : ExpressionTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<string, ConstantType> constantTypes;

            public ConstantSemanticsWalker(CompileResult result)
            {
                this.result = result;
                this.constantTypes = result.ConstantTypes;
            }

            public override void Walk(Constant constant)
            {
                this.AddResult(ConstantType.Unknown, constant.Id, constant);
                base.Walk(constant);
            }

            public override void Walk(ConstantSentence constantSentence)
            {
                this.AddResult(ConstantType.Logical, constantSentence.Constant.Id, constantSentence);
                base.Walk(constantSentence);
            }

            public override void Walk(CompleteFunctionDefinition completeFunctionDefinition)
            {
                this.AddResult(ConstantType.Function, completeFunctionDefinition.Constant.Id, completeFunctionDefinition);
                base.Walk(completeFunctionDefinition);
            }

            public override void Walk(CompleteRelationDefinition completeRelationDefinition)
            {
                this.AddResult(ConstantType.Relation, completeRelationDefinition.Constant.Id, completeRelationDefinition);
                base.Walk(completeRelationDefinition);
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                this.AddResult(ConstantType.Relation, implicitRelationalSentence.Relation.Id, implicitRelationalSentence);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                this.AddResult(ConstantType.Function, implicitFunctionalTerm.Function.Id, implicitFunctionalTerm);
                base.Walk(implicitFunctionalTerm);
            }

            private void AddResult(ConstantType type, string id, Expression expression)
            {
                if (!this.constantTypes.TryGetValue(id, out var value) || value == ConstantType.Unknown)
                {
                    this.constantTypes[id] = type;
                }
                else if (type != ConstantType.Unknown && value != type && value != ConstantType.Invalid)
                {
                    this.constantTypes[id] = ConstantType.Invalid;
                    this.result.AddCompilerError(expression.StartCursor, () => Resources.GDL002_ERROR_InconsistentConstantSemantics, id, value, type);
                }
            }
        }
    }
}
