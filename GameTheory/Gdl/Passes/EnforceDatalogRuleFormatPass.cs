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
        public const string RuleHeadMustBeAtomicError = "GDL005";
        public const string RuleBodyMustBeAtomicError = "GDL006";

        public EnforceDatalogRuleFormatPass()
        {
        }

        public override IList<string> BlockedByErrors => new[]
        {
            ReportInconsistentArityPass.InconsistentArityError,
        };

        public override IList<string> ErrorsProduced => new[]
        {
            RuleHeadMustBeAtomicError,
            RuleBodyMustBeAtomicError,
        };

        public override void Run(CompileResult result)
        {
            var allContainedVariables = new Dictionary<Expression, HashSet<Variable>>();
            var dependencyGraph = new Dictionary<Constant, Node>();
            new RuleDefinitionWalker(result, allContainedVariables, dependencyGraph).Walk((Expression)result.KnowledgeBase);
        }

        private class RuleDefinitionWalker : ExpressionTreeWalker
        {
            private readonly CompileResult result;
            private readonly Dictionary<string, int> constantArities;
            private readonly Dictionary<Sentence, bool> atomicSentences;
            private readonly Dictionary<Term, bool> datalogTerms;
            private readonly Dictionary<Sentence, bool> datalogLiterals;
            private readonly Dictionary<Expression, bool> groundExpressions;
            private readonly Dictionary<Expression, HashSet<Variable>> allContainedVariables;
            private readonly Dictionary<Constant, Node> dependencyGraph;
            private HashSet<Variable> containedVariables = new HashSet<Variable>();
            private int depth = -1;
            private bool groundExpression;

            public RuleDefinitionWalker(CompileResult result, Dictionary<Expression, HashSet<Variable>> allContainedVariables, Dictionary<Constant, Node> dependencyGraph)
            {
                this.result = result;
                this.constantArities = result.ConstantArities;
                this.atomicSentences = result.AtomicSentences;
                this.datalogTerms = result.DatalogTerms;
                this.datalogLiterals = result.DatalogLiterals;
                this.groundExpressions = result.GroundExpressions;
                this.allContainedVariables = allContainedVariables;
                this.dependencyGraph = dependencyGraph;
            }

            public override void Walk(Expression expression)
            {
                this.depth++;
                try
                {
                    var originalVariables = this.containedVariables;
                    var original = this.groundExpression;

                    this.containedVariables = new HashSet<Variable>();
                    this.groundExpression = true;

                    base.Walk(expression);

                    this.allContainedVariables[expression] = this.containedVariables;
                    this.groundExpressions[expression] = this.groundExpression;

                    originalVariables.UnionWith(this.containedVariables);
                    this.containedVariables = originalVariables;
                    this.groundExpression &= original;
                }
                finally
                {
                    this.depth--;
                }
            }

            public override void Walk(Variable variable)
            {
                this.groundExpression = false;
                this.containedVariables.Add(variable);
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

                if (this.depth == 1)
                {
                    if (!this.atomicSentences[implication.Consequent])
                    {
                        this.result.AddCompilerError(implication.Consequent.StartCursor, () => Resources.GDL005_ERROR_RuleHeadMustBeAtomic);
                    }
                    else if (!implication.Antecedents.All(a => this.datalogLiterals[a]))
                    {
                        foreach (var a in implication.Antecedents)
                        {
                            if (!this.datalogLiterals[a])
                            {
                                this.result.AddCompilerError(implication.Consequent.StartCursor, () => Resources.GDL006_ERROR_RuleBodyMustBeAtomic);
                            }
                        }
                    }
                    else
                    {
                        // TODO: Recursion restriction.
                        ////if (!this.dependencyGraph.TryGetValue(key, out Node node))
                        ////{
                        ////    node = this.dependencyGraph[key] = new Node(key);
                        ////}
                    }
                }
            }

            public override void Walk(Sentence sentence)
            {
                base.Walk(sentence);

                var atomicSentence =
                    (sentence is ConstantSentence constantSentence &&
                        this.result.ConstantTypes[constantSentence.Constant.Id] == ConstantType.Logical) ||
                    (sentence is Conjunction conjunction && conjunction.Conjuncts.All(c => this.atomicSentences[c])) ||
                    (sentence is Disjunction disjunction && disjunction.Disjuncts.All(d => this.atomicSentences[d])) ||
                    (sentence is ImplicitRelationalSentence implicitRelationalSentence &&
                        implicitRelationalSentence.SequenceVariable == null &&
                        implicitRelationalSentence.Arguments.All(a => this.datalogTerms[a]));

                this.atomicSentences[sentence] = atomicSentence;

                this.datalogLiterals[sentence] = atomicSentence || (sentence is Negation negation && this.atomicSentences[negation.Negated]);
            }
        }

        private class Edge
        {
            public Edge(string name, bool negated)
            {
                this.Name = name;
                this.Negated = negated;
            }

            public string Name { get; }

            public bool Negated { get; }
        }

        private class Node
        {
            public Node(string name)
            {
                this.Name = name;
                this.Edges = new List<Edge>();
            }

            public string Name { get; }

            public List<Edge> Edges { get; }

            public int Index { get; set; }

            public int LowLink { get; set; }
        }
    }
}
