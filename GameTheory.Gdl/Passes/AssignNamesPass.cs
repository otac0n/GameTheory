namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    internal class AssignNamesPass : CompilePass
    {
        public override IList<string> BlockedByErrors => new[]
        {
            EnforceGdlRestrictions.RoleRelationUsedInRuleError,
            EnforceGdlRestrictions.InitRelationUsedInRuleBodyError,
            EnforceGdlRestrictions.InitRelationDependencyError,
            EnforceGdlRestrictions.TrueRelationUsedOutsideRuleBodyError,
            EnforceGdlRestrictions.NextRelationUsedOutsideRuleHeadError,
            EnforceGdlRestrictions.DoesUsedOutsideRuleBodyError,
            EnforceGdlRestrictions.DoesRelationDependencyError,
        };

        public override IList<string> ErrorsProduced => new[] {
            "GDL099", // TODO: Convert to constant, etc.
        };

        public override void Run(CompileResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Name))
            {
                result.Name = "Game";
            }
            else
            {
                result.Name = result.Name.Replace('.', '_').Replace('-', '_');
                if (!char.IsLetter(result.Name[0]))
                {
                    result.Name = "_" + result.Name;
                }
            }

            AssignArgumentNames(result.KnowledgeBase, result.AssignedTypes);

            // TODO: Enable renaming variables.
            ////result.KnowledgeBase = (KnowledgeBase)new VariableReplacer(result).Walk((Expression)result.KnowledgeBase);
        }

        private static void AssignArgumentNames(KnowledgeBase knowledgeBase, AssignedTypes assignedTypes)
        {
            var argumentVariables = assignedTypes.ExpressionTypes.Values.Where(e => e is ExpressionWithArgumentsInfo).ToDictionary(e => (ExpressionWithArgumentsInfo)e, e => new List<VariableInfo[]>());
            new VariableNameWalker(assignedTypes, argumentVariables).Walk((Expression)knowledgeBase);
            var variableComparer = new StringArrayEqualityComparer(StringComparer.Ordinal);
            foreach (var kvp in argumentVariables)
            {
                var expressionInfo = kvp.Key;
                var names = kvp.Value;

                var fixedVariables = new string[expressionInfo.Arity];
                var allCandidates = new HashSet<string[]>(variableComparer);
                allCandidates.UnionWith(names.Select(n => n.Select(v => v?.Id).ToArray()));

                while (fixedVariables.Any(v => v is null) && allCandidates.Count > 0)
                {
                    var candidateList = allCandidates.ToList();
                    var votes = new int[candidateList.Count];

                    for (var b = 0; b < expressionInfo.Arity; b++)
                    {
                        if (fixedVariables[b] != null)
                        {
                            continue;
                        }

                        for (var i = 0; i < names.Count; i++)
                        {
                            var name = names[i][b];
                            if (name is null)
                            {
                                continue;
                            }

                            for (var c = 0; c < candidateList.Count; c++)
                            {
                                var cName = candidateList[c][b];
                                if (cName is null || string.Equals(name.Id, cName, StringComparison.Ordinal))
                                {
                                    votes[c]++;
                                }
                            }
                        }
                    }

                    var winners = (from i in Enumerable.Range(0, candidateList.Count)
                                   let score = votes[i]
                                   let c = candidateList[i]
                                   group c by score into g
                                   orderby g.Key descending
                                   select g).First();
                    var winner = winners.First(); // TODO: Tie-breaks.

                    var conflicting = new HashSet<string[]>(variableComparer);

                    for (var b = 0; b < expressionInfo.Arity; b++)
                    {
                        if (fixedVariables[b] is null && winner[b] != null && !fixedVariables.Any(v => v == winner[b]))
                        {
                            fixedVariables[b] = winner[b];
                            conflicting.UnionWith(allCandidates.Where(c => c[b] != null));
                        }
                    }

                    conflicting.UnionWith(allCandidates.Where(candidate => candidate.Any(c => c != null && fixedVariables.Any(v => v == c))));

                    allCandidates.ExceptWith(conflicting);
                    foreach (var conflict in conflicting)
                    {
                        var cleaned = Enumerable.Range(0, expressionInfo.Arity).Select(b => fixedVariables[b] != null || fixedVariables.Any(v => v == conflict[b]) ? null : conflict[b]).ToArray();
                        if (cleaned.Any(c => c != null))
                        {
                            allCandidates.Add(cleaned);
                        }
                    }
                }

                var scope = new Scope<VariableInfo>();
                for (var b = 0; b < expressionInfo.Arity; b++)
                {
                    var argument = expressionInfo.Arguments[b];
                    scope = scope.Add(
                        argument,
                        ScopeFlags.Private | ScopeFlags.Public,
                        fixedVariables[b],
                        argument.ReturnType.Name);
                    argument.Id = scope.TryGetPrivate(argument);
                }

                expressionInfo.Scope = scope;
            }
        }

        /// <summary>
        /// Replaces variables in an <see cref="Expression"/> tree.
        /// </summary>
        internal class VariableReplacer : ExpressionTreeReplacer
        {
            private readonly CompileResult result;
            private Dictionary<IndividualVariable, IndividualVariable> replacements;

            /// <summary>
            /// Initializes a new instance of the <see cref="VariableReplacer"/> class.
            /// </summary>
            /// <param name="replacements">The replacements to perform.</param>
            public VariableReplacer(CompileResult result)
            {
                this.result = result;
            }

            /// <inheritdoc/>
            public override Expression Walk(KnowledgeBase knowledgeBase, ImmutableStack<string> path)
            {
                var forms = knowledgeBase.Forms;

                for (var f = 0; f < forms.Count; f++)
                {
                    var sentence = (Sentence)forms[f];
                    var implicated = sentence.GetImplicatedSentence();
                    var args = implicated is ImplicitRelationalSentence implicitRelationalSentence
                        ? implicitRelationalSentence.Arguments
                        : ImmutableList<Term>.Empty;
                    var arity = args.Count;
                    if (arity == 0)
                    {
                        continue;
                    }

                    var relationInfo = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[sentence.GetImplicatedConstantWithArity()];
                    var parameters = relationInfo.Arguments;

                    var replacements = new Dictionary<IndividualVariable, IndividualVariable>();
                    ////var parameterEquality = new List<(ArgumentInfo, Term)>();
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var arg = args[i];
                        var param = parameters[i];
                        if (arg is IndividualVariable argVar)
                        {
                            if (param.Id != argVar.Id)
                            {
                                if (replacements.TryGetValue(argVar, out var matching))
                                {
                                    ////parameterEquality.Add((param, matching));
                                }
                                else
                                {
                                    replacements[argVar] = new IndividualVariable(param.Id);
                                }
                            }
                        }
                        else
                        {
                            ////parameterEquality.Add((param, arg));
                        }
                    }

                    // TODO: Add renames to avoid name collisions.
                    var collisions = new HashSet<IndividualVariable>(replacements.Values);
                    collisions.ExceptWith(replacements.Keys);

                    this.replacements = replacements;
                    var replaced = (Sentence)this.Walk((Expression)sentence);
                    this.replacements = null;

                    if (replaced != sentence)
                    {
                        var sentenceVars = this.result.AssignedTypes.VariableTypes[sentence];

                        // TODO: Update the contained variables.
                        // TODO: Update the sentence variables.
                        // TODO: Update expression type bodies.
                        forms = forms.SetItem(f, replaced);
                    }
                }

                return forms != knowledgeBase.Forms
                    ? new KnowledgeBase(forms)
                    : knowledgeBase;
            }

            /// <inheritdoc/>
            public override Expression Walk(IndividualVariable individualVariable, ImmutableStack<string> path) =>
                this.replacements.TryGetValue(individualVariable, out var replacement)
                    ? replacement
                    : individualVariable;
        }

        private class VariableNameWalker : SupportedExpressionsTreeWalker
        {
            private readonly ImmutableDictionary<(string, int), ExpressionInfo> expressionTypes;
            private readonly ImmutableDictionary<Sentence, ImmutableDictionary<IndividualVariable, VariableInfo>> containedVariables;
            private readonly Dictionary<ExpressionWithArgumentsInfo, List<VariableInfo[]>> variableNames;
            private ImmutableDictionary<IndividualVariable, VariableInfo> variableTypes;

            public VariableNameWalker(AssignedTypes assignedTypes, Dictionary<ExpressionWithArgumentsInfo, List<VariableInfo[]>> variableNames)
            {
                this.expressionTypes = assignedTypes.ExpressionTypes;
                this.containedVariables = assignedTypes.VariableTypes;
                this.variableNames = variableNames;
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                var relationInfo = (RelationInfo)this.expressionTypes[(implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count)];
                this.AddNameUsages(implicitRelationalSentence.Arguments, relationInfo);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                var functionInfo = (FunctionInfo)this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];
                this.AddNameUsages(implicitFunctionalTerm.Arguments, functionInfo);
                base.Walk(implicitFunctionalTerm);
            }

            public override void Walk(KnowledgeBase knowledgeBase)
            {
                foreach (var form in knowledgeBase.Forms)
                {
                    this.variableTypes = this.containedVariables[(Sentence)form];
                    this.Walk((Expression)form);
                    this.variableTypes = null;
                }
            }

            private void AddNameUsages(ImmutableList<Term> arguments, ExpressionWithArgumentsInfo relationInfo)
            {
                var arity = arguments.Count;

                var variables = new VariableInfo[arity];
                var anyFound = false;
                for (var i = 0; i < arity; i++)
                {
                    var source = relationInfo.Arguments[i];
                    var target = this.GetExpressionInfo(arguments[i]);

                    if ((variables[i] = target as VariableInfo) != null)
                    {
                        anyFound = true;
                    }
                }

                if (anyFound)
                {
                    (this.variableNames.TryGetValue(relationInfo, out var results)
                        ? results
                        : this.variableNames[relationInfo] = new List<VariableInfo[]>())
                        .Add(variables);
                }
            }

            private ExpressionInfo GetExpressionInfo(Term arg)
            {
                switch (arg)
                {
                    case Constant constant:
                        return this.expressionTypes[(constant.Id, 0)];

                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];

                    case IndividualVariable individualVariable:
                        return this.variableTypes[individualVariable];

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private class StringArrayEqualityComparer : IEqualityComparer<string[]>
        {
            private readonly StringComparer stringComparer;

            public StringArrayEqualityComparer(StringComparer stringComparer)
            {
                this.stringComparer = stringComparer;
            }

            public bool Equals(string[] x, string[] y) =>
                object.ReferenceEquals(x, y) || (!(x is null) && !(y is null) && x.Length == y.Length && Enumerable.Range(0, x.Length).All(i => this.stringComparer.Equals(x[i], y[i])));

            public int GetHashCode(string[] obj) => obj is null ? 0 :
                Enumerable.Range(0, obj.Length).Aggregate(HashUtilities.Seed, (hash, i) =>
                {
                    var str = obj[i];
                    HashUtilities.Combine(ref hash, str is null ? 0 : this.stringComparer.GetHashCode());
                    return hash;
                });
        }
    }
}
