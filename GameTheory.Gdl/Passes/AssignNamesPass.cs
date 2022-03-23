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

        public override IList<string> ErrorsProduced => new[]
        {
            "GDL099", // TODO: Convert to constant, etc.
        };

        public override void Run(CompileResult result)
        {
            var allTypes = new List<ExpressionType>();
            var allExpressions = new List<ExpressionInfo>();
            ExpressionTypeVisitor.Visit(result.AssignedTypes, visitExpression: allExpressions.Add, visitType: allTypes.Add);

            var globalScope = new Scope<object>()
                .Add(result, ScopeFlags.Public, result.Name, "Game");
            result.Name = globalScope.GetPublic(result);

            var namespaceScope = new Scope<object>()
                .Reserve(result.Name)
                .Reserve("IXml")
                .Reserve("TimeSpan")
                .Reserve("Math")
                .Reserve("MaximizingPlayer")
                .Reserve("TimeSpan")
                .Reserve("ResultScore")
                .Reserve("IGameStateScoringMetric")
                .Reserve("ScoringMetric")
                .Add(result.Name + "MaximizingPlayer", ScopeFlags.Public, result.Name + "MaximizingPlayer")
                .Add("NameLookup", ScopeFlags.Public, "NameLookup")
                .Add("GameState", ScopeFlags.Public, "GameState")
                .Add("Move", ScopeFlags.Public, "Move", $"{result.Name} Move", "RoleMove")
                .Add("ObjectComparer", ScopeFlags.Public, "ObjectComparer");
            namespaceScope = allTypes.Aggregate(namespaceScope, (scope, type) =>
            {
                switch (type)
                {
                    case EnumType enumType:
                        return scope.Add(type, ScopeFlags.Public, enumType.RelationInfo.Constant.Name);

                    case NoneType noneType:
                        return scope.Add(type, ScopeFlags.Public, "None");

                    case StateType _:
                        return scope.Add(type, ScopeFlags.Public, "State");

                    case FunctionType functionType:
                        return scope.Add(functionType, ScopeFlags.Public, functionType.FunctionInfo.Constant.Name);
                }

                return scope;
            });

            var gameStateScope = new Scope<object>()
                .AddPrivate("moves", "moves")
                .Reserve("CompareTo")
                .Reserve("FormatTokens")
                .Reserve(namespaceScope.GetPublic("GameState"))
                .Reserve("GetAvailableMoves")
                .Reserve("GetOutcomes")
                .Reserve("GetView")
                .Reserve("GetScore")
                .Reserve("GetWinners")
                .Reserve("Players")
                .Reserve("MakeMove")
                .Reserve("ToString")
                .Reserve("ToXml");

            var role = (RelationInfo)result.AssignedTypes.ExpressionTypes[KnownConstants.Role];
            var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;
            var noop = result.AssignedTypes.ExpressionTypes.TryGetValue(KnownConstants.Noop, out var noopExpr) ? (ObjectInfo)noopExpr : null;
            if (roles.Count > 1 && noop != null)
            {
                gameStateScope = gameStateScope.Add("FindForcedNoOps", ScopeFlags.Public, "FindForcedNoOps");
            }

            var dependencies = result.DependencyGraph;
            var seen = new HashSet<(Constant, int)> { KnownConstants.Next };
            var doesQueue = new Queue<(Constant, int)>();
            doesQueue.Enqueue(KnownConstants.Does);
            while (doesQueue.Count > 0)
            {
                var relation = doesQueue.Dequeue();
                if (seen.Add(relation))
                {
                    if (dependencies.TryGetValue(relation, out var value))
                    {
                        foreach (var dependency in value.dependencies)
                        {
                            doesQueue.Enqueue(dependency);
                        }
                    }
                }
            }

            foreach (var expr in allExpressions)
            {
                switch (expr)
                {
                    case FunctionInfo functionInfo:
                        functionInfo.Scope = new Scope<object>();
                        break;

                    case ObjectInfo objectInfo when objectInfo.Value is int:
                        break;

                    case ConstantInfo constantInfo:
                        if (constantInfo is RelationInfo relationInfo)
                        {
                            relationInfo.Scope = new Scope<object>();
                            if (seen.Contains((relationInfo.Constant, relationInfo.Arity)))
                            {
                                relationInfo.Scope = relationInfo.Scope.AddPrivate("moves", "moves");
                            }
                        }
                        else if (constantInfo is LogicalInfo logicalInfo)
                        {
                            logicalInfo.Scope = new Scope<object>();
                            if (seen.Contains((logicalInfo.Constant, 0)))
                            {
                                logicalInfo.Scope = logicalInfo.Scope.AddPrivate("moves", "moves");
                            }
                        }

                        gameStateScope = gameStateScope.Add(expr, ScopeFlags.Public, constantInfo.Constant.Name);
                        break;
                }
            }

            result.GlobalScope = globalScope;
            result.NamespaceScope = namespaceScope;
            result.GameStateScope = gameStateScope;

            AssignArgumentNames(result);
        }

        private static void AssignArgumentNames(CompileResult result)
        {
            var argumentVariables = result.AssignedTypes.ExpressionTypes.Values.Where(e => e is ExpressionWithArgumentsInfo).ToDictionary(e => (ExpressionWithArgumentsInfo)e, e => new List<VariableInfo[]>());
            new VariableNameWalker(result.AssignedTypes, argumentVariables).Walk((Expression)result.KnowledgeBase);
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

                var scope = expressionInfo.Scope
                    .Reserve(expressionInfo is FunctionInfo functionInfo
                        ? result.NamespaceScope.TryGetPublic(functionInfo.ReturnType)
                        : result.GameStateScope.TryGetPublic(expressionInfo));
                for (var b = 0; b < expressionInfo.Arity; b++)
                {
                    var argument = expressionInfo.Arguments[b];
                    scope = scope.Add(
                        argument,
                        ScopeFlags.Private | ScopeFlags.Public,
                        fixedVariables[b],
                        argument.ReturnType?.ToString());
                    argument.Id = scope.GetPrivate(argument);
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

        private class VariableNameWalker : SupportedExpressionsTreeWalker
        {
            private readonly ImmutableDictionary<Sentence, ImmutableDictionary<IndividualVariable, VariableInfo>> containedVariables;
            private readonly ImmutableDictionary<(Constant, int), ConstantInfo> expressionTypes;
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
                var relationInfo = (RelationInfo)this.expressionTypes[(implicitRelationalSentence.Relation, implicitRelationalSentence.Arguments.Count)];
                this.AddNameUsages(implicitRelationalSentence.Arguments, relationInfo);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                var functionInfo = (FunctionInfo)this.expressionTypes[(implicitFunctionalTerm.Function, implicitFunctionalTerm.Arguments.Count)];
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
                        return this.expressionTypes[(constant, 0)];

                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.expressionTypes[(implicitFunctionalTerm.Function, implicitFunctionalTerm.Arguments.Count)];

                    case IndividualVariable individualVariable:
                        return this.variableTypes[individualVariable];

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
