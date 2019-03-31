// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System.Collections.Generic;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    internal class ReportInconsistentArityPass : CompilePass
    {
        public const string InconsistentArityError = "GDL003";
        public const string SequenceVariablesUnsupportedError = "GDL004";

        public override IList<string> BlockedByErrors => new[]
        {
            ReportUnsupportedExpressionTypesPass.UnsupportedExpressionTypeError,
            ReportInconsistentConstantSemanticsPass.InconsistentConstantSemanticsError,
        };

        public override IList<string> ErrorsProduced => new[]
        {
            InconsistentArityError,
            SequenceVariablesUnsupportedError,
        };

        public override void Run(CompileResult result)
        {
            new ReportInconsistentArityWalker(result).Walk((Expression)result.KnowledgeBase);
        }

        private class ReportInconsistentArityWalker : ExpressionTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<string, int> constantArities;

            public ReportInconsistentArityWalker(CompileResult result)
            {
                this.result = result;
                this.constantArities = result.ConstantArities;
            }

            public override void Walk(SequenceVariable sequenceVariable)
            {
                this.result.AddCompilerError(sequenceVariable.StartCursor, () => Resources.GDL004_ERROR_SequenceVariablesUnsupported);
            }

            public override void Walk(CompleteFunctionDefinition completeFunctionDefinition)
            {
                this.AddResult(
                    completeFunctionDefinition.Constant.Id,
                    completeFunctionDefinition.Parameters.Count,
                    completeFunctionDefinition);
                base.Walk(completeFunctionDefinition);
            }

            public override void Walk(CompleteRelationDefinition completeRelationDefinition)
            {
                this.AddResult(
                    completeRelationDefinition.Constant.Id,
                    completeRelationDefinition.Parameters.Count,
                    completeRelationDefinition);
                base.Walk(completeRelationDefinition);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                this.AddResult(
                    implicitFunctionalTerm.Function.Id,
                    implicitFunctionalTerm.Arguments.Count,
                    implicitFunctionalTerm);
                base.Walk(implicitFunctionalTerm);
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                this.AddResult(
                    implicitRelationalSentence.Relation.Id,
                    implicitRelationalSentence.Arguments.Count,
                    implicitRelationalSentence);
                base.Walk(implicitRelationalSentence);
            }

            private void AddResult(string id, int arity, Expression expression)
            {
                if (!this.constantArities.TryGetValue(id, out var value) || value == -1)
                {
                    this.constantArities[id] = arity;
                }
                else if (value != arity && arity != -1)
                {
                    this.constantArities[id] = -1;
                    this.result.AddCompilerError(expression.StartCursor, () => Resources.GDL003_ERROR_InconsistentArity, id, value, arity);
                }
            }
        }
    }
}
