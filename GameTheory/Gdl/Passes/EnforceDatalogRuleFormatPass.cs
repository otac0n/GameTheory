// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    internal class EnforceDatalogRuleFormatPass : CompilePass
    {
        public EnforceDatalogRuleFormatPass()
        {
        }

        public override IList<string> BlockedByErrors => new[]
        {
            ReportInconsistentArityPass.InconsistentArityError,
        };

        public override IList<string> ErrorsProduced => Array.Empty<string>();

        public override void Run(CompileResult result)
        {
            new RuleDefinitionWalker(result).Walk((Expression)result.KnowledgeBase);
        }

        private class RuleDefinitionWalker : ExpressionTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<string, int> constantArities;
            private readonly Dictionary<Sentence, bool> atomicSentences;
            private readonly Dictionary<Term, bool> datalogTerms;
            private readonly Dictionary<Implication, bool> datalogRules;
            private readonly Dictionary<Sentence, bool> datalogLiterals;
            private readonly Dictionary<Expression, bool> groundExpressions;
            private bool groundExpression;

            public RuleDefinitionWalker(CompileResult result)
            {
                this.result = result;
                this.constantArities = result.ConstantArities;
                this.atomicSentences = result.AtomicSentences;
                this.datalogTerms = result.DatalogTerms;
                this.datalogRules = result.DatalogRules;
                this.datalogLiterals = result.DatalogLiterals;
                this.groundExpressions = result.GroundExpressions;
            }

            public override void Walk(Expression expression)
            {
                var original = this.groundExpression;
                this.groundExpression = true;
                base.Walk(expression);
                this.groundExpressions[expression] = this.groundExpression;
                this.groundExpression &= original;
            }

            public override void Walk(Variable variable)
            {
                this.groundExpression = false;
                base.Walk(variable);
            }

            public override void Walk(Term term)
            {
                base.Walk(term);

                this.datalogTerms[term] =
                    term is Constant ||
                    term is IndividualVariable ||
                    (term is ImplicitFunctionalTerm functionalTerm &&
                        functionalTerm.Arguments.Count == this.constantArities[functionalTerm.Function.Id] &&
                        functionalTerm.Arguments.All(a => this.datalogTerms[a]));
            }

            public override void Walk(Implication implication)
            {
                base.Walk(implication);
                this.datalogRules[implication] =
                    this.atomicSentences[implication.Consequent] &&
                    implication.Antecedents.All(a => this.datalogLiterals[a]);

                // TODO: Safety requirement.
            }

            public override void Walk(Sentence sentence)
            {
                base.Walk(sentence);

                var atomicSentence =
                    sentence is ImplicitRelationalSentence implicitRelationalSentence &&
                    implicitRelationalSentence.SequenceVariable == null &&
                    implicitRelationalSentence.Arguments.All(a => this.datalogTerms[a]);

                this.atomicSentences[sentence] = atomicSentence;

                this.datalogLiterals[sentence] = atomicSentence || (sentence is Negation negation && this.atomicSentences[negation]);
            }
        }
    }
}
