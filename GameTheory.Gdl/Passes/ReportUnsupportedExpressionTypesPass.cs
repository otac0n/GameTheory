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
            private int depth;

            public UnsupportedExpressionTypeWalker(CompileResult result)
            {
                this.depth = -1;
                this.result = result;
            }

            public override void Walk(Expression expression)
            {
                this.depth++;
                try
                {
                    var supported = true;
                    switch (this.depth)
                    {
                        case 0:
                            supported = expression is KnowledgeBase;
                            break;

                        case 1:
                            switch (expression)
                            {
                                case ConstantSentence constantSentence:
                                case ImplicitRelationalSentence implicitRelationalSentence:
                                case Implication implication:
                                    break;

                                default:
                                    supported = false;
                                    break;
                            }

                            break;

                        default:
                            switch (expression)
                            {
                                case Quotation quotation:
                                case SequenceVariable sequenceVariable:
                                case Definition definition:
                                case CharacterBlock characterBlock:
                                case CharacterReference characterReference:
                                case CharacterString characterString:
                                case LogicalTerm logicalTerm:
                                case Equation equation:
                                case Equivalence equivalence:
                                case QuantifiedSentence quantifiedSentence:
                                case ExplicitFunctionalTerm explicitFunctionalTerm:
                                case ExplicitRelationalSentence explicitRelationalSentence:
                                case Inequality inequality:
                                case ListExpression listExpression:
                                case ListTerm listTerm:
                                case LogicalPair logicalPair:
                                case VariableSpecification variableSpecification:
                                case Implication implication:
                                    supported = false;
                                    break;
                            }

                            break;
                    }

                    if (!supported)
                    {
                        this.result.AddCompilerError(expression.StartCursor, () => Resources.GDL001_ERROR_UnsupportedExpressionType, expression.GetType().Name);
                        return;
                    }

                    base.Walk(expression);
                }
                finally
                {
                    this.depth--;
                }
            }
        }
    }
}
