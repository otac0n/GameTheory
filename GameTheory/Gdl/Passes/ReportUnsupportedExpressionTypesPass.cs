// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    internal class ReportUnsupportedExpressionTypesPass : CompilePass
    {
        public const string UnsupportedExpressionTypeError = "GDL001";

        public override IList<string> BlockedByErrors => Array.Empty<string>();

        public override IList<string> ErrorsProduced => new[]
        {
            UnsupportedExpressionTypeError,
        };

        public override void Run(CompileResult result)
        {
            new UnsupportedExpressionTypeWalker(result).Walk((Expression)result.KnowledgeBase);
        }

        private class UnsupportedExpressionTypeWalker : ExpressionTreeWalker
        {
            private readonly CompileResult result;

            public UnsupportedExpressionTypeWalker(CompileResult result)
            {
                this.result = result;
            }

            public override void Walk(Expression expression)
            {
                switch (expression)
                {
                    case ExistentiallyQuantifiedSentence existentiallyQuantifiedSentence:
                    case ExplicitFunctionalTerm explicitFunctionalTerm:
                    case ExplicitRelationalSentence explicitRelationalSentence:
                    case UnrestrictedDefinition unrestrictedDefinition:
                    case PartialDefinition partialDefinition:
                        this.result.AddCompilerError(expression.StartCursor, () => Resources.GDL001_ERROR_UnsupportedExpressionType, expression.GetType().Name);
                        break;
                }

                base.Walk(expression);
            }
        }
    }
}
