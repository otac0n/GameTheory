// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System.Collections.Generic;
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
        }

        private class ConstantSemanticsWalker : SupportedExpressionsTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<(string, int), ConstantType> constantTypes;

            public ConstantSemanticsWalker(CompileResult result)
            {
                this.result = result;
                this.constantTypes = result.ConstantTypes;
            }

            public override void Walk(Constant constant)
            {
                this.CheckError((constant.Id, 0), ConstantType.Object, constant);
                base.Walk(constant);
            }

            public override void Walk(ConstantSentence constantSentence)
            {
                this.CheckError((constantSentence.Constant.Id, 0), ConstantType.Logical, constantSentence);
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                this.CheckError((implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count), ConstantType.Relation, implicitRelationalSentence);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                this.CheckError((implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count), ConstantType.Function, implicitFunctionalTerm);
                base.Walk(implicitFunctionalTerm);
            }

            private void CheckError((string id, int arity) key, ConstantType type, Expression expression)
            {
                if (this.constantTypes[key] == ConstantType.Invalid)
                {
                    this.result.AddCompilerError(expression.StartCursor, () => Resources.GDL002_ERROR_InconsistentConstantSemantics, key.id, key.arity, type);
                }
            }
        }
    }
}
