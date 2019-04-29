// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System.Collections.Generic;
    using System.Linq;
    using KnowledgeInterchangeFormat.Expressions;

    internal class EnforceDatalogRuleFormatPass : CompilePass
    {
        public const string RuleHeadMustBeAtomicError = "GDL005";
        public const string RuleBodyMustBeAtomicError = "GDL006";

        public EnforceDatalogRuleFormatPass()
        {
        }

        public override IList<string> BlockedByErrors => new[]
        {
            ReportInconsistentConstantSemanticsPass.InconsistentConstantSemanticsError,
        };

        public override IList<string> ErrorsProduced => new[]
        {
            RuleHeadMustBeAtomicError,
            RuleBodyMustBeAtomicError,
        };

        public override void Run(CompileResult result)
        {
            new RuleDefinitionWalker(result).Walk((Expression)result.KnowledgeBase);
        }

        private class RuleDefinitionWalker : SupportedExpressionsTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<Sentence, bool> atomicSentences;
            private readonly Dictionary<Term, bool> datalogTerms;
            private readonly Dictionary<Sentence, bool> datalogLiterals;
            private int depth = -1;
            private bool groundExpression;

            public RuleDefinitionWalker(CompileResult result)
            {
                this.result = result;
                this.atomicSentences = result.AtomicSentences;
                this.datalogTerms = result.DatalogTerms;
                this.datalogLiterals = result.DatalogLiterals;
            }

            public override void Walk(Expression expression)
            {
                this.depth++;
                try
                {
                    base.Walk(expression);
                }
                finally
                {
                    this.depth--;
                }
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
                    (term is ImplicitFunctionalTerm functionalTerm && functionalTerm.Arguments.All(a => this.datalogTerms[a]));
            }

            public override void Walk(Implication implication)
            {
                base.Walk(implication);

                if (this.depth == 1)
                {
                    if (!this.atomicSentences[implication.Consequent])
                    {
                        this.result.AddCompilerError(implication.Consequent.StartCursor, () => Resources.GDL005_ERROR_RuleHeadMustBeAtomic);
                    }
                    else
                    {
                        void EnsureCombinationOfDatalogLiterals(Sentence sentence)
                        {
                            if (this.datalogLiterals[sentence])
                            {
                            }
                            else if (sentence is Conjunction conjunction)
                            {
                                foreach (var conjunct in conjunction.Conjuncts)
                                {
                                    EnsureCombinationOfDatalogLiterals(conjunct);
                                }
                            }
                            else if (sentence is Disjunction disjunction)
                            {
                                foreach (var disjunct in disjunction.Disjuncts)
                                {
                                    EnsureCombinationOfDatalogLiterals(disjunct);
                                }
                            }
                            else
                            {
                                this.result.AddCompilerError(sentence.StartCursor, () => Resources.GDL006_ERROR_RuleBodyMustBeAtomic);
                            }
                        }

                        foreach (var antecedent in implication.Antecedents)
                        {
                            EnsureCombinationOfDatalogLiterals(antecedent);
                        }
                    }
                }
            }

            public override void Walk(Sentence sentence)
            {
                base.Walk(sentence);

                var atomicSentence =
                    (sentence is ConstantSentence constantSentence &&
                        this.result.ConstantTypes[(constantSentence.Constant, 0)] == ConstantType.Logical) ||
                    (sentence is ImplicitRelationalSentence implicitRelationalSentence &&
                        implicitRelationalSentence.SequenceVariable == null &&
                        implicitRelationalSentence.Arguments.All(a => this.datalogTerms[a]));

                this.atomicSentences[sentence] = atomicSentence;

                this.datalogLiterals[sentence] = atomicSentence || (sentence is Negation negation && this.atomicSentences[negation.Negated]);
            }
        }
    }
}
