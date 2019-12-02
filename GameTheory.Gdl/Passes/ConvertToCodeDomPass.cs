// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using Intervals;
    using KnowledgeInterchangeFormat.Expressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct ExpressionScope
    {
        public ExpressionScope(ImmutableDictionary<IndividualVariable, VariableInfo> variables, Scope<object> scope, ImmutableDictionary<VariableInfo, ExpressionSyntax> names)
        {
            this.Variables = variables;
            this.Scope = scope;
            this.Names = names;
        }

        public ImmutableDictionary<VariableInfo, ExpressionSyntax> Names { get; }

        public Scope<object> Scope { get; }

        public ImmutableDictionary<IndividualVariable, VariableInfo> Variables { get; }
    }

    internal class ConvertToCodeDomPass : CompilePass
    {
        public const string ErrorConvertingToCodeDomError = "GDL101";

        private static readonly Lazy<IList<string>> EarlierErrors = new Lazy<IList<string>>(() =>
        {
            return (from p in typeof(Resources).GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetProperty)
                    let parts = p.Name.Split('_')
                    where parts.Length == 3
                    where parts[1] == "ERROR"
                    where string.Compare(parts[0], ErrorConvertingToCodeDomError, StringComparison.InvariantCulture) < 0
                    select parts[0])
                    .ToList()
                    .AsReadOnly();
        });

        /// <inheritdoc/>
        public override IList<string> BlockedByErrors => EarlierErrors.Value;

        /// <inheritdoc/>
        public override IList<string> ErrorsProduced => new[]
        {
            ErrorConvertingToCodeDomError,
        };

        /// <inheritdoc/>
        public override void Run(CompileResult result)
        {
            try
            {
                new Runner(result).Run();
            }
            catch (Exception ex)
            {
                result.AddCompilerError(result.KnowledgeBase.StartCursor, () => Resources.GDL101_ERROR_ErrorConvertingToCodeDom, ex.ToString());
            }
        }

        private class Runner
        {
            private readonly CompileResult result;

            public Runner(CompileResult result)
            {
                this.result = result;
            }

            public static TypeSyntax ReferenceBuiltIn(Type type)
            {
                if (type == typeof(object))
                {
                    return SyntaxHelper.ObjectType;
                }
                else if (type == typeof(void))
                {
                    return SyntaxHelper.VoidType;
                }
                else if (type == typeof(int))
                {
                    return SyntaxHelper.IntType;
                }
                else if (type == typeof(bool))
                {
                    return SyntaxHelper.BoolType;
                }
                else if (type == typeof(string))
                {
                    return SyntaxHelper.StringType;
                }

                return SyntaxFactory.ParseTypeName(type.FullName);
            }

            public ExpressionSyntax AllMembers(ExpressionType type, ExpressionType declaredAs, ExpressionScope scope)
            {
                switch (type)
                {
                    case AnyType anyType:
                        return SyntaxFactory.ArrayCreationExpression(
                            SyntaxHelper.ArrayType(this.Reference(declaredAs)),
                            SyntaxFactory.InitializerExpression(
                                SyntaxKind.ArrayInitializerExpression)
                                .AddExpressions(this.result.GroundTerms.Select(e => this.ConvertExpression(e, scope)).ToArray()));

                    case FunctionType functionType:
                        {
                            var functionValues = this.result.GroundTerms.Where(t => this.result.AssignedTypes.GetExpressionInfo(t, scope.Variables).ReturnType == functionType);

                            return SyntaxFactory.ArrayCreationExpression(
                                SyntaxHelper.ArrayType(this.Reference(declaredAs)),
                                SyntaxFactory.InitializerExpression(
                                    SyntaxKind.ArrayInitializerExpression)
                                    .AddExpressions(functionValues.Select(e => this.ConvertExpression(e, scope)).ToArray()));
                        }

                    case NoneType noneType:
                        return SyntaxFactory.ArrayCreationExpression(
                            SyntaxHelper.ArrayType(this.Reference(declaredAs)),
                            SyntaxFactory.InitializerExpression(
                                SyntaxKind.ArrayInitializerExpression,
                                SyntaxFactory.SeparatedList<ExpressionSyntax>()));

                    case ObjectType objectType:
                        return SyntaxFactory.ArrayCreationExpression(
                            SyntaxHelper.ArrayType(this.Reference(declaredAs)),
                            SyntaxFactory.InitializerExpression(
                                SyntaxKind.ArrayInitializerExpression,
                                SyntaxFactory.SingletonSeparatedList(
                                    this.CreateObjectReference(objectType.ObjectInfo))));

                    case NumberRangeType numberRangeType:
                        {
                            var numberValues = SyntaxHelper.EnumerableRangeExpression(numberRangeType.Start, numberRangeType.End - numberRangeType.Start + 1);

                            if (numberRangeType == declaredAs)
                            {
                                return numberValues;
                            }

                            var subScope = scope.Scope.SubScope<object>()
                                .AddPrivate("v", "v");
                            return SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    numberValues,
                                    SyntaxHelper.IdentifierName("Select")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.SimpleLambdaExpression(
                                                SyntaxFactory.Parameter(
                                                    SyntaxFactory.Identifier(subScope.GetPrivate("v"))),
                                                SyntaxFactory.CastExpression(
                                                    this.Reference(declaredAs),
                                                    SyntaxFactory.IdentifierName(subScope.GetPrivate("v"))))));
                        }

                    case EnumType enumType:
                        {
                            var enumValues = SyntaxFactory.ParenthesizedExpression(
                                SyntaxFactory.CastExpression(
                                    SyntaxHelper.ArrayType(this.Reference(enumType)),
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxHelper.IdentifierName("Enum"),
                                            SyntaxHelper.IdentifierName("GetValues")),
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.TypeOfExpression(
                                                        SyntaxFactory.ParseTypeName(this.result.NamespaceScope.GetPublic(enumType)))))))));

                            if (enumType == declaredAs)
                            {
                                return enumValues;
                            }

                            var subScope = scope.Scope.SubScope<object>()
                                .AddPrivate("v", "v");
                            return SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    enumValues,
                                    SyntaxHelper.IdentifierName("Select")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.SimpleLambdaExpression(
                                                SyntaxFactory.Parameter(
                                                    SyntaxFactory.Identifier(subScope.GetPrivate("v"))),
                                                SyntaxFactory.CastExpression(
                                                    this.Reference(declaredAs),
                                                    SyntaxFactory.IdentifierName(subScope.GetPrivate("v"))))));
                        }

                    case UnionType unionType:
                        var numbers = unionType.Expressions.ToLookup(e => e.ReturnType is NumberRangeType);
                        var numberRanges = (numbers[true].Select(e => (NumberRangeType)e.ReturnType).Simplify() ?? Array.Empty<IInterval<int>>()).Select(r => (NumberRangeType)r);
                        var nonNumbersGrouped = numbers[false].GroupBy(e => e.ReturnType);
                        var nonNumbersByExplicitList = nonNumbersGrouped.ToLookup(g => g.Key is EnumType enumType && g.All(e => e is ObjectInfo) && !enumType.Objects.SetEquals(g.Cast<ObjectInfo>()));

                        var numberExpressions = numberRanges;
                        var explicitListExpressions = nonNumbersByExplicitList[true].Select(g =>
                            SyntaxFactory.ArrayCreationExpression(
                                SyntaxHelper.ArrayType(this.Reference(declaredAs)),
                                SyntaxFactory.InitializerExpression(
                                    SyntaxKind.ArrayInitializerExpression)
                                    .AddExpressions(g.Cast<ObjectInfo>().Select(e => this.CreateObjectReference(e)).ToArray())));
                        var restExpressions = numberRanges.Concat(nonNumbersByExplicitList[false].Select(g => g.Key)).Select(t => this.AllMembers(t, declaredAs, scope));

                        return explicitListExpressions.Concat(restExpressions)
                            .Aggregate((a, b) =>
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        a,
                                        SyntaxHelper.IdentifierName("Concat")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    b)))));
                }

                throw new NotSupportedException($"Could not enumerate the members of the type '{type}'");
            }

            public TypeSyntax Reference(ExpressionType type)
            {
                if (type is BuiltInType builtIn)
                {
                    return ReferenceBuiltIn(builtIn.Type);
                }
                else if (type.StorageType != type)
                {
                    return this.Reference(type.StorageType);
                }

                switch (type)
                {
                    case EnumType enumType:
                    case FunctionType functionType:
                        var typeName = this.result.NamespaceScope.GetPublic(type);
                        var typeReference = SyntaxHelper.IdentifierName(typeName);
                        if (this.result.GameStateScope.ContainsName(typeName))
                        {
                            var namespaceName = this.result.GlobalScope.GetPublic(this.result);
                            var namespaceReference = SyntaxHelper.IdentifierName(namespaceName);
                            if (this.result.GameStateScope.ContainsName(namespaceName))
                            {
                                return SyntaxFactory.QualifiedName(
                                    SyntaxFactory.AliasQualifiedName(
                                        SyntaxFactory.IdentifierName(
                                            SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                        namespaceReference),
                                    typeReference);
                            }
                            else
                            {
                                return SyntaxFactory.QualifiedName(
                                    namespaceReference,
                                    typeReference);
                            }
                        }
                        else
                        {
                            return typeReference;
                        }

                    case StateType stateType:
                        return SyntaxHelper.ObjectType;
                }

                throw new NotSupportedException($"Could not reference the type '{type}'");
            }

            public void Run()
            {
                var allTypes = new List<ExpressionType>();
                var allExpressions = new List<ExpressionInfo>();
                ExpressionTypeVisitor.Visit(this.result.AssignedTypes, visitExpression: allExpressions.Add, visitType: allTypes.Add);

                var init = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Init];
                var @true = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.True];
                var next = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Next];
                var role = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Role];
                var does = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Does];
                var legal = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Legal];
                var goal = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Goal];
                var distinct = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Distinct];
                var terminal = (LogicalInfo)this.result.AssignedTypes.ExpressionTypes[KnownConstants.Terminal];
                var noop = this.result.AssignedTypes.ExpressionTypes.TryGetValue(KnownConstants.Noop, out var noopExpr) ? (ObjectInfo)noopExpr : null;
                var stateType = (StateType)init.Arguments[0].ReturnType;
                var moveType = legal.Arguments[1].ReturnType;

                var renderedTypes = allTypes.Where(t =>
                    !(t is BuiltInType) &&
                    !(t.StorageType != t));
                var renderedExpressions = allExpressions.Except(init, next).Where(e =>
                    !(e is VariableInfo) &&
                    !(e is FunctionInfo) &&
                    !(e is ObjectInfo objectInfo && (objectInfo.Value is int || objectInfo.ReturnType is EnumType)));

                var root = SyntaxFactory.CompilationUnit();

                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxHelper.IdentifierName(this.result.Name))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(
                            SyntaxHelper.IdentifierName("System")),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxHelper.IdentifierName("System"),
                                    SyntaxHelper.IdentifierName("Collections")),
                                SyntaxHelper.IdentifierName("Generic"))),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxHelper.IdentifierName("System"),
                                SyntaxHelper.IdentifierName("Linq"))),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxHelper.IdentifierName("System"),
                                    SyntaxHelper.IdentifierName("Threading")),
                                SyntaxHelper.IdentifierName("Tasks"))),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxHelper.IdentifierName("System"),
                                SyntaxHelper.IdentifierName("Xml"))),
                        SyntaxFactory.UsingDirective(
                            SyntaxHelper.IdentifierName("GameTheory")),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxHelper.IdentifierName("GameTheory"),
                                    SyntaxHelper.IdentifierName("Gdl")),
                                SyntaxHelper.IdentifierName("Shared"))));

                var move = this.CreateMoveTypeDeclaration(does.Arguments[1].ReturnType);

                var gameState = SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.GetPublic("GameState"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.GenericName(
                                SyntaxHelper.Identifier("IGameState"))
                            .AddTypeArgumentListArguments(
                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")))),
                        SyntaxFactory.SimpleBaseType(
                            SyntaxHelper.IdentifierName("ITokenFormattable")),
                        SyntaxFactory.SimpleBaseType(
                            SyntaxHelper.IdentifierName("IXml")))
                    .AddMembers(this.CreateGameStateConstructorDeclarations(init, stateType, role, moveType, noop))
                    .AddMembers(
                        this.CreateMakeMoveDeclaration(next, stateType, role, noop),
                        this.CreateGetScoreDeclaration(goal, role),
                        this.CreateGetWinnersDeclaration(goal, role, terminal),
                        this.CreateGetAvailableMovesDeclaration(legal, role, terminal))
                    .AddMembers(this.CreateSharedGameStateDeclarations())
                    .AddMembers(
                        renderedExpressions.Select(expr =>
                        {
                            switch (expr)
                            {
                                case ObjectInfo objectInfo:
                                    return this.CreateObjectDeclaration(objectInfo, (string)objectInfo.Value);

                                case RelationInfo relationInfo:
                                    if (relationInfo == @true)
                                    {
                                        Debug.Assert(relationInfo.Body.Count == 0, "The true relation is not defined by the game.");
                                        return this.CreateTrueRelationDeclaration(@true);
                                    }
                                    else if (relationInfo == distinct)
                                    {
                                        Debug.Assert(relationInfo.Body.Count == 0, "The distinct relation is not defined by the game.");
                                        return this.CreateDistinctRelationDeclaration(distinct);
                                    }
                                    else if (relationInfo == does)
                                    {
                                        Debug.Assert(relationInfo.Body.Count == 0, "The does relation is not defined by the game.");
                                        return this.CreateDoesRelationDeclaration(does);
                                    }
                                    else
                                    {
                                        return this.CreateLogicalFunctionDeclaration(relationInfo, relationInfo.Arguments, relationInfo.Body);
                                    }

                                case LogicalInfo logicalInfo:
                                    return this.CreateLogicalFunctionDeclaration(logicalInfo, Array.Empty<ArgumentInfo>(), logicalInfo.Body);
                            }

                            throw new InvalidOperationException();
                        }).ToArray());

                gameState = SyntaxHelper.ReorderMembers(gameState);

                ns = ns
                    .AddMembers(gameState, move)
                    .AddMembers(
                        this.CreatePublicTypeDeclarations(renderedTypes))
                    .AddMembers(
                        this.CreateObjectComparerDeclaration(allTypes));
                root = root.AddMembers(ns);

                this.result.DeclarationSyntax = root.NormalizeWhitespace();
            }

            private static ObjectCreationExpressionSyntax NewPlayerTokenExpression() =>
                SyntaxFactory.ObjectCreationExpression(SyntaxHelper.IdentifierName("PlayerToken"))
                    .WithArgumentList(SyntaxFactory.ArgumentList());

            private ExpressionSyntax ConvertCondition(Sentence condition, ref ExpressionScope scope)
            {
                switch (condition)
                {
                    case ImplicitRelationalSentence implicitRelationalSentence:
                        return this.ConvertImplicitRelationalCondition(implicitRelationalSentence, ref scope);

                    case ConstantSentence constantSentence:
                        return this.ConvertLogicalCondition(constantSentence, scope);

                    case Negation negation:
                        return this.ConvertNegationCondition(negation, ref scope);

                    case Disjunction disjunction:
                        return this.ConvertDisjunctionCondition(disjunction, ref scope);

                    default:
                        throw new NotSupportedException($"Could not convert condition '{condition}'");
                }
            }

            private StatementSyntax[] ConvertConjnuction(ImmutableList<Sentence> conjuncts, Func<ExpressionScope, StatementSyntax[]> inner, ExpressionScope scope)
            {
                StatementSyntax[] GetStatement(int i, ExpressionScope s1)
                {
                    if (i >= conjuncts.Count)
                    {
                        return inner(s1);
                    }

                    return new[] { this.ConvertSentence(conjuncts[i], s2 => GetStatement(i + 1, s2), s1) };
                }

                return GetStatement(0, scope);
            }

            private ExpressionSyntax ConvertConstantExpression(Constant constant) =>
                this.CreateObjectReference((ObjectInfo)this.result.AssignedTypes.ExpressionTypes[(constant, 0)]);

            private ExpressionSyntax ConvertDisjunctionCondition(Disjunction disjunction, ref ExpressionScope scope)
            {
                ExpressionSyntax expr = null;

                foreach (var d in disjunction.Disjuncts)
                {
                    var next = this.ConvertCondition(d, ref scope);
                    expr = expr == null ? next : SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, expr, next);
                }

                return expr;
            }

            private ExpressionSyntax ConvertExpression(Term term, ExpressionScope scope)
            {
                switch (term)
                {
                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.ConvertFunctionalTermExpression(implicitFunctionalTerm, scope);

                    case IndividualVariable individualVariable:
                        return this.ConvertVariableExpression(individualVariable, scope);

                    case Constant constant:
                        return this.ConvertConstantExpression(constant);

                    default:
                        throw new NotSupportedException($"Could not convert expression '{term}'");
                }
            }

            private ExpressionSyntax ConvertFunctionalTermExpression(ImplicitFunctionalTerm implicitFunctionalTerm, ExpressionScope scope) =>
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic(this.result.AssignedTypes.GetExpressionInfo(implicitFunctionalTerm, scope.Variables).ReturnType)))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitFunctionalTerm.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));

            private StatementSyntax[] ConvertImplicatedSentences(IEnumerable<Sentence> sentences, Func<Sentence, ExpressionScope, StatementSyntax[]> getImplication, Scope<object> scope, ImmutableDictionary<VariableInfo, ExpressionSyntax> names)
            {
                var shareableConjuncts = (from sentence in sentences
                                          let s1 = new ExpressionScope(this.result.AssignedTypes.VariableTypes[sentence], scope, names)
                                          let implicated = sentence.GetImplicatedSentence()
                                          let conjuncts = sentence is Implication
                                              ? ((Implication)sentence).Antecedents
                                              : ImmutableList<Sentence>.Empty
                                          let shareable = (from conjunct in conjuncts
                                                           where this.result.ContainedVariables[conjunct].IsEmpty // TODO: Allow grouping by variables.
                                                           select conjunct).ToImmutableList()
                                          let implication = new { scope = s1, conjuncts, implicated }
                                          select new { implication, shareable }).ToList();

                // Allows us to declare a recursive lambda with an anonymous type.
                Func<T, Func<Sentence, ExpressionScope, StatementSyntax[]>, StatementSyntax[]> Describe<T>(T template) => (ignore0, ignore1) => null;
                var convertImplicated = Describe(shareableConjuncts);
                convertImplicated = (sc, inner) =>
                {
                    var conjunctCounts = (from pair in sc
                                          from shared in pair.shareable
                                          group pair.implication by shared).ToDictionary(g => g.Key, g => g.Count());

                    var sharedGroups = (from pair in sc
                                        let max = pair.shareable.OrderByDescending(s => conjunctCounts[s]).FirstOrDefault()
                                        group pair by max into g
                                        let gList = g.ToList()
                                        select new
                                        {
                                            Key = gList.Count == 1 ? null : g.Key,
                                            Remaining = g.Key == null || gList.Count == 1
                                                ? gList
                                                : g.Select(pair => new
                                                {
                                                    implication = new
                                                    {
                                                        pair.implication.scope,
                                                        conjuncts = pair.implication.conjuncts.Remove(g.Key),
                                                        pair.implication.implicated,
                                                    },
                                                    shareable = pair.shareable.Remove(g.Key),
                                                }).ToList(),
                                        }).ToList();

                    var statements = new List<StatementSyntax>();

                    foreach (var group in sharedGroups)
                    {
                        if (group.Key != null)
                        {
                            statements.AddRange(
                                this.ConvertConjnuction(
                                    ImmutableList.Create(group.Key),
                                    s2 => convertImplicated(
                                        group.Remaining.Select(r => new
                                        {
                                            implication = new
                                            {
                                                r.implication.scope, // TODO: Allow grouping by variables.
                                                r.implication.conjuncts,
                                                r.implication.implicated,
                                            },
                                            r.shareable,
                                        }).ToList(),
                                        inner),
                                    new ExpressionScope(null, scope, null))); // TODO: Allow grouping by variables.
                        }
                        else
                        {
                            foreach (var pair in group.Remaining)
                            {
                                var s1 = pair.implication.scope;
                                var conjuncts = pair.implication.conjuncts;
                                var implicated = pair.implication.implicated;

                                statements.AddRange(
                                    this.ConvertConjnuction(
                                        conjuncts,
                                        s2 => inner(implicated, s2),
                                        s1));
                            }
                        }
                    }

                    return statements.ToArray();
                };

                return convertImplicated(shareableConjuncts, getImplication);
            }

            private ExpressionSyntax ConvertImplicitRelationalCondition(ImplicitRelationalSentence implicitRelationalSentence, ref ExpressionScope scope)
            {
                var relationInfo = (RelationInfo)this.result.AssignedTypes.GetExpressionInfo(implicitRelationalSentence, scope.Variables);

                var invocation = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(),
                            SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(relationInfo))));

                var conditions = new List<ExpressionSyntax>();
                for (var i = 0; i < relationInfo.Arguments.Length; i++)
                {
                    var param = relationInfo.Arguments[i];
                    var arg = implicitRelationalSentence.Arguments[i];
                    var argInfo = this.result.AssignedTypes.GetExpressionInfo(arg, scope.Variables);
                    var pStorage = param.ReturnType.StorageType;
                    var aStorage = argInfo.ReturnType.StorageType;

                    if (!ExpressionType.IsAssignableFrom(pStorage, aStorage))
                    {
                        scope = new ExpressionScope(
                            scope.Variables,
                            scope.Scope.AddPrivate(out var var, $"{arg} as {pStorage}"),
                            scope.Names);

                        conditions.Add(
                            SyntaxFactory.IsPatternExpression(
                                this.ConvertExpression(arg, scope),
                                SyntaxFactory.DeclarationPattern(
                                    this.Reference(param.ReturnType),
                                    SyntaxFactory.SingleVariableDesignation(
                                        SyntaxHelper.Identifier(var)))));

                        invocation = invocation.AddArgumentListArguments(SyntaxFactory.Argument(SyntaxHelper.IdentifierName(var)));
                    }
                    else
                    {
                        invocation = invocation.AddArgumentListArguments(SyntaxFactory.Argument(this.ConvertExpression(arg, scope)));
                    }
                }

                if (relationInfo.Scope.ContainsKey("moves"))
                {
                    if (scope.Scope.ContainsKey("moves"))
                    {
                        invocation = invocation.AddArgumentListArguments(
                            SyntaxFactory.Argument(
                                SyntaxHelper.IdentifierName(scope.Scope.GetPrivate("moves"))));
                    }
                    else
                    {
                        invocation = invocation.AddArgumentListArguments(
                            SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves")))));
                    }
                }

                if (conditions.Count > 0)
                {
                    return SyntaxFactory.ParenthesizedExpression(
                        conditions.Concat(new[] { invocation }).Aggregate((a, b) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, a, b)));
                }
                else
                {
                    return invocation;
                }
            }

            private ExpressionSyntax ConvertLogicalCondition(ConstantSentence constantSentence, ExpressionScope scope)
            {
                var logicalInfo = (LogicalInfo)this.result.AssignedTypes.GetExpressionInfo(constantSentence, scope.Variables);

                var invocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(logicalInfo))));

                if (logicalInfo.Scope.ContainsKey("moves"))
                {
                    if (scope.Scope.ContainsKey("moves"))
                    {
                        invocation = invocation.AddArgumentListArguments(
                            SyntaxFactory.Argument(
                                SyntaxHelper.IdentifierName(scope.Scope.GetPrivate("moves"))));
                    }
                    else
                    {
                        invocation = invocation.AddArgumentListArguments(
                            SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves")))));
                    }
                }

                return invocation;
            }

            // TODO: Runtime type checks
            private ExpressionSyntax ConvertNegationCondition(Negation negation, ref ExpressionScope scope) =>
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    this.ConvertCondition(negation.Negated, ref scope));

            private StatementSyntax ConvertSentence(Sentence sentence, Func<ExpressionScope, StatementSyntax[]> inner, ExpressionScope scope)
            {
                return this.FixVariables(
                    sentence,
                    s1 => SyntaxFactory.IfStatement(
                        this.ConvertCondition(sentence, ref s1),
                        SyntaxFactory.Block(inner(s1)))
                        .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}"))),
                    scope);
            }

            private ExpressionSyntax ConvertVariableExpression(IndividualVariable individualVariable, ExpressionScope scope)
            {
                if (!scope.Variables.ContainsKey(individualVariable))
                {
                    return SyntaxFactory.IdentifierName(individualVariable.Name);
                }

                var variable = scope.Variables[individualVariable];
                if (!scope.Names.ContainsKey(variable))
                {
                    return SyntaxFactory.IdentifierName(variable.Id);
                }

                return scope.Names[variable];
            }

            private MemberDeclarationSyntax CreateDistinctRelationDeclaration(RelationInfo distinct) =>
                SyntaxFactory.MethodDeclaration(
                    this.Reference(distinct.ReturnType),
                    this.result.GameStateScope.GetPublic(distinct))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxHelper.Identifier("a"))
                            .WithType(SyntaxHelper.ObjectType),
                        SyntaxFactory.Parameter(SyntaxHelper.Identifier("b"))
                            .WithType(SyntaxHelper.ObjectType))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.PrefixUnaryExpression(
                                        SyntaxKind.LogicalNotExpression,
                                        SyntaxHelper.ObjectEqualsExpression(SyntaxHelper.IdentifierName("a"), SyntaxHelper.IdentifierName("b")))))));

            private MemberDeclarationSyntax CreateDoesRelationDeclaration(RelationInfo does)
            {
                return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxHelper.Identifier(this.result.GameStateScope.GetPublic(does)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier("role"))
                            .WithType(
                                this.Reference(does.Arguments[0].ReturnType)),
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier("move"))
                            .WithType(
                                this.Reference(does.Arguments[1].ReturnType)),
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier(does.Scope.GetPrivate("moves")))
                            .WithType(
                                SyntaxHelper.ArrayType(
                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")))))
                    .AddBodyStatements(
                        SyntaxFactory.ReturnStatement(
                            SyntaxHelper.ObjectEqualsExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ElementAccessExpression(
                                        SyntaxHelper.IdentifierName(does.Scope.GetPrivate("moves")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.CastExpression(
                                                    SyntaxHelper.IntType,
                                                    SyntaxHelper.IdentifierName("role")))),
                                    SyntaxHelper.IdentifierName("Value")),
                                SyntaxHelper.IdentifierName("move"))));
            }

            private ClassDeclarationSyntax CreateEnumLookupDeclaration(IList<EnumType> enumTypes)
            {
                var lookupElement = SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.GetPublic("NameLookup"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));

                foreach (var enumType in enumTypes)
                {
                    var switchStatement = SyntaxFactory.SwitchStatement(
                        SyntaxFactory.IdentifierName("value"))
                            .WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                            .WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken));

                    foreach (var obj in enumType.Objects)
                    {
                        switchStatement = switchStatement
                            .AddSections(
                                SyntaxFactory.SwitchSection()
                                    .AddLabels(
                                        SyntaxFactory.CaseSwitchLabel(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.GetPublic(enumType)),
                                                SyntaxFactory.IdentifierName(enumType.Scope.GetPublic(obj)))))
                                    .AddStatements(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxHelper.LiteralExpression(obj.Constant.Name))));
                    }

                    switchStatement = switchStatement
                        .AddSections(
                            SyntaxFactory.SwitchSection()
                                .AddLabels(SyntaxFactory.DefaultSwitchLabel())
                                .AddStatements(SyntaxFactory.ReturnStatement(SyntaxHelper.Null)));

                    var enumElement = SyntaxFactory.MethodDeclaration(SyntaxHelper.StringType, SyntaxFactory.Identifier("GetName"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("value"))
                                .WithType(
                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.GetPublic(enumType))))
                        .AddBodyStatements(switchStatement);

                    lookupElement = lookupElement
                        .AddMembers(enumElement);
                }

                return lookupElement;
            }

            private EnumDeclarationSyntax CreateEnumTypeDeclaration(EnumType enumType)
            {
                var enumElement = SyntaxFactory.EnumDeclaration(this.result.NamespaceScope.GetPublic(enumType))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var obj in enumType.Objects)
                {
                    enumElement = enumElement.AddMembers(
                        SyntaxFactory.EnumMemberDeclaration(enumType.Scope.GetPublic(obj)));
                }

                return enumElement;
            }

            private StructDeclarationSyntax CreateFunctionTypeDeclaration(FunctionType functionType)
            {
                var functionInfo = functionType.FunctionInfo;

                var structElement = SyntaxFactory.StructDeclaration(this.result.NamespaceScope.GetPublic(functionInfo.ReturnType))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxHelper.IdentifierName("ITokenFormattable")),
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.GenericName(
                                SyntaxHelper.Identifier("IComparable"))
                            .AddTypeArgumentListArguments(
                                this.Reference(functionType))));

                if (functionInfo.Arguments.Length > 0)
                {
                    var constructor = SyntaxFactory.ConstructorDeclaration(structElement.Identifier)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block());

                    foreach (var arg in functionInfo.Arguments)
                    {
                        var type = this.Reference(arg.ReturnType);
                        var fieldVariable = functionInfo.Scope.GetPrivate(arg);
                        var fieldElement = SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                type,
                                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxHelper.Identifier(fieldVariable)))))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                        var parameter = SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier(fieldVariable))
                            .WithType(type);
                        constructor = constructor.AddParameterListParameters(parameter);

                        constructor = constructor.AddBodyStatements(SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxHelper.IdentifierName(fieldVariable)),
                                SyntaxFactory.IdentifierName(parameter.Identifier))));

                        var propElement = SyntaxFactory.PropertyDeclaration(type, functionInfo.Scope.GetPublic(arg))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration,
                                    SyntaxFactory.Block(
                                        SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxHelper.IdentifierName(fieldVariable))))));

                        structElement = structElement.AddMembers(fieldElement, propElement);
                    }

                    structElement = structElement.AddMembers(constructor);
                }

                var compareTo = SyntaxFactory.MethodDeclaration(
                    SyntaxHelper.IntType,
                    SyntaxHelper.Identifier("CompareTo"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(
                                    SyntaxHelper.Identifier("other"))
                                    .WithType(
                                        this.Reference(functionType)))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IntType)
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier("comp"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxHelper.LiteralExpression(0)))))));

                if (functionInfo.Arguments.Length > 0)
                {
                    compareTo = compareTo
                        .AddBodyStatements(
                            SyntaxFactory.IfStatement(
                                functionInfo.Arguments.Select(arg =>
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.NotEqualsExpression,
                                        SyntaxFactory.ParenthesizedExpression(
                                            SyntaxFactory.AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxHelper.IdentifierName("comp"),
                                                arg.ReturnType.StorageType is AnyType
                                                    ? SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("ObjectComparer")),
                                                                SyntaxHelper.IdentifierName("Instance")),
                                                            SyntaxHelper.IdentifierName("Compare")))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.ThisExpression(),
                                                                    SyntaxHelper.IdentifierName(functionInfo.Scope.GetPrivate(arg)))),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxHelper.IdentifierName("other"),
                                                                    SyntaxHelper.IdentifierName(functionInfo.Scope.GetPrivate(arg)))))
                                                    : SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ThisExpression(),
                                                                SyntaxHelper.IdentifierName(functionInfo.Scope.GetPrivate(arg))),
                                                            SyntaxHelper.IdentifierName("CompareTo")))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxHelper.IdentifierName("other"),
                                                                    SyntaxHelper.IdentifierName(functionInfo.Scope.GetPrivate(arg))))))),
                                        SyntaxHelper.LiteralExpression(0)))
                                    .Aggregate((a, b) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, a, b)),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxHelper.IdentifierName("comp"))))));
                }

                compareTo = compareTo
                    .AddBodyStatements(
                        SyntaxFactory.ReturnStatement(
                            SyntaxHelper.IdentifierName("comp")));

                var formatTokens =
                    SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxHelper.Identifier("IList"))
                        .AddTypeArgumentListArguments(
                            SyntaxHelper.ObjectType),
                        SyntaxHelper.Identifier("FormatTokens"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.ArrayCreationExpression(
                                    SyntaxHelper.ArrayType(
                                        SyntaxHelper.ObjectType))
                                    .WithInitializer(
                                        SyntaxFactory.InitializerExpression(
                                            SyntaxKind.ArrayInitializerExpression)
                                            .AddExpressions(
                                                SyntaxHelper.LiteralExpression("("),
                                                SyntaxHelper.LiteralExpression(functionInfo.Constant.Name))
                                            .AddExpressions(
                                                functionInfo.Arguments.SelectMany(arg => new ExpressionSyntax[]
                                                {
                                                    SyntaxHelper.LiteralExpression(" "),
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.ThisExpression(),
                                                        SyntaxHelper.IdentifierName(functionInfo.Scope.GetPrivate(arg))),
                                                }).ToArray())
                                            .AddExpressions(
                                                SyntaxHelper.LiteralExpression(")")))))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                structElement = structElement.AddMembers(formatTokens, compareTo);

                var toXmlBody = SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxHelper.IdentifierName("writer"),
                                    SyntaxHelper.IdentifierName("WriteStartElementAsync")))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NullLiteralExpression)),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("relation"))),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NullLiteralExpression))))),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxHelper.IdentifierName("writer"),
                                    SyntaxHelper.IdentifierName("WriteStringAsync")))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxHelper.LiteralExpression(functionInfo.Constant.Name))))),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxHelper.IdentifierName("writer"),
                                    SyntaxHelper.IdentifierName("WriteEndElementAsync"))))));
                foreach (var arg in functionInfo.Arguments)
                {
                    toXmlBody = toXmlBody.AddStatements(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxHelper.IdentifierName("writer"),
                                        SyntaxHelper.IdentifierName("WriteStartElementAsync")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NullLiteralExpression)),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal("argument"))),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NullLiteralExpression))))),
                        this.CreateToXmlStatements(
                            arg.ReturnType,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxHelper.IdentifierName(functionInfo.Scope.GetPrivate(arg))),
                            SyntaxHelper.IdentifierName("writer")),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxHelper.IdentifierName("writer"),
                                        SyntaxHelper.IdentifierName("WriteEndElementAsync"))))));
                }

                structElement = structElement.AddMembers(
                    SyntaxFactory.MethodDeclaration(
                        SyntaxHelper.IdentifierName("Task"),
                        SyntaxFactory.Identifier("ToXml"))
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("writer"))
                                .WithType(
                                    SyntaxHelper.IdentifierName("XmlWriter")))
                        .WithBody(toXmlBody));

                return SyntaxHelper.ReorderMembers(structElement);
            }

            private MemberDeclarationSyntax[] CreateGameStateConstructorDeclarations(RelationInfo init, StateType stateType, RelationInfo role, ExpressionType moveType, ObjectInfo noop)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;

                var declarations = new List<MemberDeclarationSyntax>
                {
                    SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.ParseTypeName(this.result.NamespaceScope.GetPublic(stateType)))
                            .AddVariables(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxHelper.Identifier("state"))))
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                };

                if (roles.Count > 1)
                {
                    declarations.Add(
                        SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxHelper.ArrayType(
                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxHelper.Identifier(this.result.GameStateScope.GetPrivate("moves")))))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
                }

                var constructor1 = SyntaxFactory.ConstructorDeclaration(
                    SyntaxHelper.Identifier(this.result.NamespaceScope.GetPublic("GameState")))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName("Players")),
                                    SyntaxFactory.ImplicitArrayCreationExpression(
                                        SyntaxFactory.InitializerExpression(
                                            SyntaxKind.ArrayInitializerExpression))
                                        .AddInitializerExpressions(
                                            Enumerable.Range(0, roles.Count)
                                                .Select(_ => NewPlayerTokenExpression())
                                                .ToArray()))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName("state")),
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.ParseTypeName(this.result.NamespaceScope.GetPublic(stateType)))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList())))));

                constructor1 = constructor1
                    .AddBodyStatements(
                        this.ConvertImplicatedSentences(
                            init.Body,
                            (i, s1) => new[]
                            {
                                this.FixVariables(
                                    i,
                                    s2 => SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxHelper.IdentifierName("state")),
                                                SyntaxHelper.IdentifierName("Add")))
                                            .AddArgumentListArguments(
                                                SyntaxFactory.Argument(
                                                    this.ConvertExpression(((ImplicitRelationalSentence)i).Arguments[0], s2)))),
                                    s1),
                            },
                            new Scope<object>(),
                            ImmutableDictionary<VariableInfo, ExpressionSyntax>.Empty));

                var constructor2 = SyntaxFactory.ConstructorDeclaration(
                    SyntaxHelper.Identifier(this.result.NamespaceScope.GetPublic("GameState")))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier("players"))
                            .WithType(
                                SyntaxFactory.GenericName(
                                    SyntaxHelper.Identifier("IReadOnlyList"))
                                    .AddTypeArgumentListArguments(
                                        SyntaxHelper.IdentifierName("PlayerToken"))),
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier("state"))
                            .WithType(
                                SyntaxFactory.ParseTypeName(this.result.NamespaceScope.GetPublic(stateType))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName("Players")),
                                    SyntaxHelper.IdentifierName("players"))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName("state")),
                                    SyntaxHelper.IdentifierName("state")))));

                if (roles.Count > 1)
                {
                    constructor2 = constructor2
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxHelper.Identifier(this.result.GameStateScope.GetPrivate("moves")))
                                .WithType(
                                    SyntaxHelper.ArrayType(
                                        SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")))));

                    if (noop != null)
                    {
                        constructor1 = constructor1
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves"))),
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic("FindForcedNoOps"))))));

                        constructor2 = constructor2
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves"))),
                                    SyntaxHelper.Coalesce(
                                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves")),
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic("FindForcedNoOps")))))));
                    }
                    else
                    {
                        constructor1 = constructor1
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves"))),
                                    SyntaxFactory.ArrayCreationExpression(
                                        SyntaxHelper.ArrayType(
                                            SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")),
                                            SyntaxHelper.LiteralExpression(roles.Count)))));

                        constructor2 = constructor2
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves"))),
                                    SyntaxHelper.Coalesce(
                                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves")),
                                        SyntaxFactory.ArrayCreationExpression(
                                            SyntaxHelper.ArrayType(
                                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")),
                                                SyntaxHelper.LiteralExpression(roles.Count))))));
                    }
                }

                declarations.Add(constructor1);
                declarations.Add(constructor2);

                if (roles.Count > 1 && noop != null)
                {
                    declarations.Add(
                        SyntaxFactory.MethodDeclaration(
                            SyntaxHelper.ArrayType(
                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))),
                            SyntaxHelper.Identifier(this.result.GameStateScope.GetPublic("FindForcedNoOps")))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                            .WithBody(
                                SyntaxFactory.Block(
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                            SyntaxHelper.IdentifierName("var"))
                                            .AddVariables(
                                                SyntaxFactory.VariableDeclarator(
                                                    SyntaxHelper.Identifier("moves"))
                                                    .WithInitializer(
                                                        SyntaxFactory.EqualsValueClause(
                                                            SyntaxFactory.ArrayCreationExpression(
                                                                SyntaxHelper.ArrayType(
                                                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")),
                                                                    SyntaxHelper.LiteralExpression(roles.Count))))))),
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxHelper.IdentifierName("moves")))));
                }

                return declarations.ToArray();
            }

            private MethodDeclarationSyntax CreateGetAvailableMovesDeclaration(RelationInfo legal, RelationInfo role, LogicalInfo terminal)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;

                var terminalInvocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(terminal))));
                if (terminal.Scope.ContainsKey("moves"))
                {
                    terminalInvocation = terminalInvocation.AddArgumentListArguments(
                        SyntaxFactory.Argument(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves")))));
                }

                var statements = SyntaxFactory.Block(
                    this.ConvertImplicatedSentences(
                        legal.Body,
                        (i, s1) =>
                        {
                            return new[]
                            {
                                this.FixVariables(
                                    i,
                                    s2 =>
                                    {
                                        var implicated = (ImplicitRelationalSentence)i;
                                        StatementSyntax addStatement = SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxHelper.IdentifierName("moves"),
                                                    SyntaxHelper.IdentifierName("Add")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.ObjectCreationExpression(
                                                            SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")))
                                                            .AddArgumentListArguments(
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.ElementAccessExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.ThisExpression(),
                                                                            SyntaxHelper.IdentifierName("Players")))
                                                                    .AddArgumentListArguments(
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxFactory.CastExpression(
                                                                                SyntaxHelper.IntType,
                                                                                this.ConvertExpression(implicated.Arguments[0], s2))))),
                                                                SyntaxFactory.Argument(this.ConvertExpression(implicated.Arguments[1], s2))))));

                                        return roles.Count > 1
                                            ? SyntaxFactory.IfStatement(
                                                SyntaxFactory.IsPatternExpression(
                                                    SyntaxFactory.ElementAccessExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves"))))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.CastExpression(
                                                                    SyntaxHelper.IntType,
                                                                    this.ConvertExpression(implicated.Arguments[0], s2)))),
                                                    SyntaxFactory.ConstantPattern(
                                                        SyntaxHelper.Null)),
                                                SyntaxFactory.Block(
                                                    addStatement))
                                            : addStatement;
                                    },
                                    s1),
                            };
                        },
                        new Scope<object>(),
                        ImmutableDictionary<VariableInfo, ExpressionSyntax>.Empty));

                return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxHelper.Identifier("IReadOnlyList"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))))),
                    SyntaxHelper.Identifier("GetAvailableMoves"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier("moves"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.GenericName(
                                                            SyntaxHelper.Identifier("List"))
                                                            .AddTypeArgumentListArguments(
                                                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))))
                                                        .WithArgumentList(
                                                            SyntaxFactory.ArgumentList()))))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.PrefixUnaryExpression(
                                    SyntaxKind.LogicalNotExpression,
                                    terminalInvocation),
                                statements),
                            SyntaxFactory.ReturnStatement(
                            SyntaxHelper.IdentifierName("moves"))));
            }

            private MethodDeclarationSyntax CreateGetScoreDeclaration(RelationInfo goal, RelationInfo role)
            {
                var scope = new Scope<object>()
                    .AddPrivate("player", "player")
                    .AddPrivate("role", "role")
                    .AddPrivate("g", "g");

                return SyntaxFactory.MethodDeclaration(
                    SyntaxHelper.IntType,
                    SyntaxHelper.Identifier("GetScore"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier(scope.GetPrivate("player")))
                            .WithType(
                                SyntaxHelper.IdentifierName("PlayerToken")))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier(scope.GetPrivate("role")))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.CastExpression(
                                                        this.Reference(role.Arguments[0].ReturnType),
                                                        SyntaxFactory.InvocationExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.ThisExpression(),
                                                                    SyntaxFactory.IdentifierName("Players")),
                                                                SyntaxFactory.IdentifierName("IndexOf")))
                                                            .WithArgumentList(
                                                                SyntaxFactory.ArgumentList(
                                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                                        SyntaxFactory.Argument(
                                                                            SyntaxHelper.IdentifierName(scope.GetPrivate("player"))))))))))),
                            SyntaxFactory.ForStatement(
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.IfStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(goal))))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxHelper.IdentifierName(scope.GetPrivate("role"))),
                                                    SyntaxFactory.Argument(
                                                        SyntaxHelper.IdentifierName(scope.GetPrivate("g")))),
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                    SyntaxFactory.ReturnStatement(
                                                        SyntaxHelper.IdentifierName(scope.GetPrivate("g")))))))))
                                .WithDeclaration(
                                    SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.IdentifierName("var"))
                                    .WithVariables(
                                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxHelper.Identifier(scope.GetPrivate("g")))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        SyntaxFactory.Literal(100)))))))
                                .WithCondition(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.GreaterThanOrEqualExpression,
                                        SyntaxHelper.IdentifierName(scope.GetPrivate("g")),
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(1))))
                                .WithIncrementors(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.PostfixUnaryExpression(
                                            SyntaxKind.PostDecrementExpression,
                                            SyntaxHelper.IdentifierName(scope.GetPrivate("g"))))),
                            SyntaxFactory.ReturnStatement(
                                SyntaxHelper.LiteralExpression(0))));
            }

            private MethodDeclarationSyntax CreateGetWinnersDeclaration(RelationInfo goal, RelationInfo role, LogicalInfo terminal)
            {
                var scope = new Scope<object>()
                    .AddPrivate("winners", "winners")
                    .AddPrivate("role", "role")
                    .AddPrivate("g", "g");

                var terminalInvocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(terminal))));
                if (terminal.Scope.ContainsKey("moves"))
                {
                    terminalInvocation = terminalInvocation.AddArgumentListArguments(
                        SyntaxFactory.Argument(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPrivate("moves")))));
                }

                return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxHelper.Identifier("IReadOnlyList"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxHelper.IdentifierName("PlayerToken")))),
                    SyntaxHelper.Identifier("GetWinners"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier(scope.GetPrivate("winners")))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.GenericName(
                                                            SyntaxHelper.Identifier("List"))
                                                            .AddTypeArgumentListArguments(
                                                                SyntaxHelper.IdentifierName("PlayerToken")))
                                                        .WithArgumentList(
                                                            SyntaxFactory.ArgumentList()))))),
                            SyntaxFactory.IfStatement(
                                terminalInvocation,
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ForStatement(
                                            SyntaxFactory.Block(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxHelper.IdentifierName(scope.GetPrivate("winners")),
                                                            SyntaxHelper.IdentifierName("AddRange")))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.InvocationExpression(
                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                this.AllMembers(role.Arguments[0].ReturnType, role.Arguments[0].ReturnType, new ExpressionScope(ImmutableDictionary<IndividualVariable, VariableInfo>.Empty, scope, ImmutableDictionary<VariableInfo, ExpressionSyntax>.Empty)),
                                                                                SyntaxHelper.IdentifierName("Where")))
                                                                            .AddArgumentListArguments(
                                                                                SyntaxFactory.Argument(
                                                                                    SyntaxFactory.SimpleLambdaExpression(
                                                                                        SyntaxFactory.Parameter(
                                                                                            SyntaxHelper.Identifier(scope.GetPrivate("role"))),
                                                                                        SyntaxFactory.InvocationExpression(
                                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                                SyntaxFactory.ThisExpression(),
                                                                                                SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(goal))))
                                                                                            .AddArgumentListArguments(
                                                                                                SyntaxFactory.Argument(
                                                                                                    SyntaxHelper.IdentifierName(scope.GetPrivate("role"))),
                                                                                                SyntaxFactory.Argument(
                                                                                                    SyntaxHelper.IdentifierName("g")))))),
                                                                        SyntaxHelper.IdentifierName("Select")))
                                                                        .AddArgumentListArguments(
                                                                            SyntaxFactory.Argument(
                                                                                SyntaxFactory.SimpleLambdaExpression(
                                                                                    SyntaxFactory.Parameter(
                                                                                        SyntaxHelper.Identifier(scope.GetPrivate("role"))),
                                                                                    SyntaxFactory.ElementAccessExpression(
                                                                                        SyntaxFactory.MemberAccessExpression(
                                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                                            SyntaxFactory.ThisExpression(),
                                                                                            SyntaxHelper.IdentifierName("Players")))
                                                                                        .AddArgumentListArguments(
                                                                                            SyntaxFactory.Argument(
                                                                                                SyntaxFactory.CastExpression(
                                                                                                    SyntaxHelper.IntType,
                                                                                                    SyntaxHelper.IdentifierName(scope.GetPrivate("role"))))))))))),
                                                SyntaxFactory.IfStatement(
                                                    SyntaxFactory.BinaryExpression(
                                                        SyntaxKind.GreaterThanExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxHelper.IdentifierName(scope.GetPrivate("winners")),
                                                            SyntaxHelper.IdentifierName("Count")),
                                                        SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(0))),
                                                    SyntaxFactory.Block(
                                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                                            SyntaxFactory.BreakStatement())))))
                                            .WithDeclaration(
                                                SyntaxFactory.VariableDeclaration(
                                                    SyntaxHelper.IdentifierName("var"))
                                                    .WithVariables(
                                                        SyntaxFactory.SingletonSeparatedList(
                                                            SyntaxFactory.VariableDeclarator(
                                                                SyntaxHelper.Identifier(scope.GetPrivate("g")))
                                                                .WithInitializer(
                                                                    SyntaxFactory.EqualsValueClause(
                                                                        SyntaxHelper.LiteralExpression(100))))))
                                            .WithCondition(
                                                SyntaxFactory.BinaryExpression(
                                                    SyntaxKind.GreaterThanOrEqualExpression,
                                                    SyntaxHelper.IdentifierName(scope.GetPrivate("g")),
                                                    SyntaxHelper.LiteralExpression(1)))
                                            .WithIncrementors(
                                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                    SyntaxFactory.PostfixUnaryExpression(
                                                        SyntaxKind.PostDecrementExpression,
                                                        SyntaxHelper.IdentifierName(scope.GetPrivate("g")))))))),
                            SyntaxFactory.ReturnStatement(
                                SyntaxHelper.IdentifierName(scope.GetPrivate("winners")))));
            }

            private MethodDeclarationSyntax CreateLogicalFunctionDeclaration(ExpressionInfo expression, ArgumentInfo[] parameters, IEnumerable<Sentence> sentences)
            {
                var nameScope = expression is RelationInfo relationInfo
                    ? relationInfo.Scope
                    : expression is LogicalInfo logicalInfo
                        ? logicalInfo.Scope
                        : new Scope<object>();

                var methodElement = SyntaxFactory.MethodDeclaration(
                    this.Reference(expression.ReturnType),
                    this.result.GameStateScope.GetPublic(expression))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                foreach (var param in parameters)
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxHelper.Identifier(nameScope.GetPrivate(param)))
                        .WithType(this.Reference(param.ReturnType)));
                }

                if (nameScope.ContainsKey("moves"))
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxHelper.Identifier(nameScope.GetPrivate("moves")))
                        .WithType(
                            SyntaxHelper.ArrayType(
                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")))));
                }

                var returnTrue = SyntaxFactory.ReturnStatement(SyntaxHelper.True);

                foreach (var sentence in sentences)
                {
                    var implicated = sentence.GetImplicatedSentence();

                    var walker = new ScopeWalker(
                        this.result,
                        parameters,
                        new ExpressionScope(this.result.AssignedTypes.VariableTypes[sentence], nameScope, ImmutableDictionary<VariableInfo, ExpressionSyntax>.Empty),
                        this,
                        this.ConvertExpression);
                    walker.Walk((Expression)implicated);
                    var declarations = walker.Declarations;
                    var parameterEquality = walker.ParameterEquality;

                    var conditions = sentence is Implication implication
                        ? implication.Antecedents
                        : ImmutableList<Sentence>.Empty;

                    var root = this.ConvertConjnuction(conditions, _ => new[] { returnTrue }, walker.ExpressionScope);

                    if (parameterEquality.Count > 0)
                    {
                        root = new[]
                        {
                            SyntaxFactory.IfStatement(
                                parameterEquality.Aggregate((left, right) =>
                                    SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right)),
                                SyntaxFactory.Block(root)),
                        };
                    }

                    root = new[]
                    {
                        SyntaxFactory.Block(declarations)
                            .AddStatements(root)
                            .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}"))),
                    };

                    methodElement = methodElement.AddBodyStatements(root);
                }

                methodElement = methodElement.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxHelper.False));

                return methodElement;
            }

            private MethodDeclarationSyntax CreateMakeMoveDeclaration(RelationInfo next, ExpressionType stateType, RelationInfo role, ObjectInfo noop)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;

                var makeMoveScope = new Scope<object>()
                    .Add("move", ScopeFlags.Private, "move")
                    .Add("moves", ScopeFlags.Private, "moves");
                if (roles.Count > 1)
                {
                    makeMoveScope = makeMoveScope
                        .Add("role", ScopeFlags.Private, "role");
                }

                var makeMove = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxHelper.Identifier("IGameState"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))))),
                    SyntaxHelper.Identifier("MakeMove"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(
                                    SyntaxHelper.Identifier(makeMoveScope.GetPrivate("move")))
                                    .WithType(
                                        SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))))));

                if (roles.Count > 1)
                {
                    makeMove = makeMove
                        .AddBodyStatements(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier(makeMoveScope.GetPrivate("role")))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxHelper.IdentifierName("Players")),
                                                        SyntaxHelper.IdentifierName("IndexOf")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("move")),
                                                                SyntaxHelper.IdentifierName("PlayerToken")))))))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.LogicalOrExpression,
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.NotEqualsExpression,
                                        SyntaxFactory.ElementAccessExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("moves"))))
                                            .WithArgumentList(
                                                SyntaxFactory.BracketedArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                        SyntaxFactory.Argument(
                                                            SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("role")))))),
                                        SyntaxHelper.Null),
                                    SyntaxFactory.PrefixUnaryExpression(
                                        SyntaxKind.LogicalNotExpression,
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.ThisExpression(),
                                                        SyntaxHelper.IdentifierName("GetAvailableMoves"))),
                                                SyntaxHelper.IdentifierName("Any")))
                                            .AddArgumentListArguments(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.SimpleLambdaExpression(
                                                        SyntaxFactory.Parameter(
                                                            SyntaxHelper.Identifier("m")),
                                                        SyntaxHelper.ObjectEqualsExpression(
                                                            SyntaxHelper.IdentifierName("m"),
                                                            SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("move")))))))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ThrowStatement(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxHelper.IdentifierName("ArgumentOutOfRangeException"))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.InvocationExpression(
                                                            SyntaxHelper.IdentifierName("nameof"))
                                                            .AddArgumentListArguments(
                                                                SyntaxFactory.Argument(
                                                                    SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("move")))))))))),
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier(makeMoveScope.GetPrivate("moves")))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ArrayCreationExpression(
                                                        SyntaxHelper.ArrayType(
                                                            SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")),
                                                            SyntaxHelper.LiteralExpression(roles.Count))))))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxHelper.IdentifierName("Array"),
                                        SyntaxHelper.IdentifierName("Copy")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("moves")))),
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("moves"))),
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.LiteralExpression(roles.Count)))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.ElementAccessExpression(
                                        SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("moves")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("role")))),
                                    SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("move")))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("moves")),
                                        SyntaxHelper.IdentifierName("Any")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.SimpleLambdaExpression(
                                                SyntaxFactory.Parameter(
                                                    SyntaxHelper.Identifier("m")),
                                                SyntaxFactory.IsPatternExpression(
                                                    SyntaxHelper.IdentifierName("m"),
                                                    SyntaxFactory.ConstantPattern(
                                                        SyntaxHelper.Null))))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("GameState")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxHelper.IdentifierName("Players"))),
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxHelper.IdentifierName("state"))),
                                                    SyntaxFactory.Argument(
                                                        SyntaxHelper.IdentifierName(makeMoveScope.GetPrivate("moves")))))))));
                }
                else
                {
                    makeMove = makeMove
                        .AddBodyStatements(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier(makeMoveScope.GetPrivate("moves")))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ImplicitArrayCreationExpression(
                                                        SyntaxFactory.InitializerExpression(
                                                            SyntaxKind.ArrayInitializerExpression,
                                                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                                SyntaxFactory.IdentifierName(makeMoveScope.GetPrivate("move"))))))))));
                }

                var nextIdentifier = SyntaxHelper.Identifier("next");
                var nextIdentifierName = SyntaxHelper.IdentifierName("next");

                makeMove = makeMove
                    .AddBodyStatements(
                        SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxHelper.IdentifierName("var"))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator(
                                        nextIdentifier)
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.ObjectCreationExpression(
                                                    SyntaxFactory.ParseTypeName(this.result.NamespaceScope.GetPublic(stateType)))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList()))))));

                makeMove = makeMove
                    .AddBodyStatements(
                        this.ConvertImplicatedSentences(
                            next.Body,
                            (i, s1) => new[]
                            {
                                this.FixVariables(
                                    i,
                                    s2 => SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                nextIdentifierName,
                                                SyntaxHelper.IdentifierName("Add")))
                                            .AddArgumentListArguments(
                                                SyntaxFactory.Argument(
                                                    this.ConvertExpression(((ImplicitRelationalSentence)i).Arguments[0], s1)))),
                                    s1),
                            },
                            makeMoveScope,
                            ImmutableDictionary<VariableInfo, ExpressionSyntax>.Empty));

                if (roles.Count > 1)
                {
                    makeMove = makeMove
                        .AddBodyStatements(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("GameState")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName("Players"))),
                                        SyntaxFactory.Argument(
                                            nextIdentifierName),
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.Null))));
                }
                else
                {
                    makeMove = makeMove.AddBodyStatements(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.ObjectCreationExpression(
                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("GameState")))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxHelper.IdentifierName("Players"))),
                                    SyntaxFactory.Argument(
                                        nextIdentifierName))));
                }

                return makeMove;
            }

            private ClassDeclarationSyntax CreateMoveTypeDeclaration(ExpressionType moveType) =>
                SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.GetPublic("Move"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxHelper.IdentifierName("IMove")))))
                    .AddMembers(
                        SyntaxFactory.ConstructorDeclaration(
                            SyntaxHelper.Identifier(this.result.NamespaceScope.GetPublic("Move")))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxHelper.Identifier("playerToken"))
                                    .WithType(SyntaxHelper.IdentifierName("PlayerToken")),
                                SyntaxFactory.Parameter(SyntaxHelper.Identifier("value"))
                                    .WithType(this.Reference(moveType)))
                            .WithBody(
                                SyntaxFactory.Block(
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName("PlayerToken")),
                                            SyntaxHelper.IdentifierName("playerToken"))),
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName("Value")),
                                            SyntaxHelper.IdentifierName("value"))))),
                        SyntaxFactory.PropertyDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                            SyntaxHelper.Identifier("IsDeterministic"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .WithAccessorList(
                                SyntaxFactory.AccessorList(
                                    SyntaxFactory.SingletonList<AccessorDeclarationSyntax>(
                                        SyntaxFactory.AccessorDeclaration(
                                            SyntaxKind.GetAccessorDeclaration)
                                        .WithBody(
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                    SyntaxFactory.ReturnStatement(
                                                        SyntaxHelper.True))))))),
                        SyntaxFactory.PropertyDeclaration(
                            SyntaxHelper.IdentifierName("PlayerToken"),
                            SyntaxHelper.Identifier("PlayerToken"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))),
                        SyntaxFactory.PropertyDeclaration(
                            SyntaxFactory.GenericName(
                                SyntaxHelper.Identifier("IList"))
                                .AddTypeArgumentListArguments(
                                    SyntaxHelper.ObjectType),
                            SyntaxHelper.Identifier("FormatTokens"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithBody(
                                    SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxFactory.ArrayCreationExpression(
                                                    SyntaxHelper.ArrayType(
                                                        SyntaxHelper.ObjectType))
                                                    .WithInitializer(
                                                        SyntaxFactory.InitializerExpression(
                                                            SyntaxKind.ArrayInitializerExpression,
                                                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.ThisExpression(),
                                                                    SyntaxHelper.IdentifierName("Value")))))))))),
                        SyntaxFactory.PropertyDeclaration(
                            this.Reference(moveType),
                            SyntaxHelper.Identifier("Value"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))),
                        SyntaxFactory.MethodDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                            SyntaxHelper.Identifier("Equals"))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(
                                    SyntaxHelper.Identifier("obj"))
                                    .WithType(SyntaxHelper.ObjectType))
                            .WithExpressionBody(
                                SyntaxFactory.ArrowExpressionClause(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.LogicalAndExpression,
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.LogicalAndExpression,
                                            SyntaxFactory.IsPatternExpression(
                                                SyntaxHelper.IdentifierName("obj"),
                                                SyntaxFactory.DeclarationPattern(
                                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")),
                                                    SyntaxFactory.SingleVariableDesignation(
                                                        SyntaxHelper.Identifier("other")))),
                                            SyntaxHelper.ObjectEqualsExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxHelper.IdentifierName("PlayerToken")),
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxHelper.IdentifierName("other"),
                                                    SyntaxHelper.IdentifierName("PlayerToken")))),
                                        SyntaxHelper.ObjectEqualsExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName("Value")),
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxHelper.IdentifierName("other"),
                                                SyntaxHelper.IdentifierName("Value"))))))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.MethodDeclaration(
                            SyntaxHelper.StringType,
                            SyntaxHelper.Identifier("ToString"))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                            .WithExpressionBody(
                                SyntaxFactory.ArrowExpressionClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxHelper.StringType,
                                            SyntaxHelper.IdentifierName("Concat")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.ThisExpression(),
                                                        SyntaxHelper.IdentifierName("FlattenFormatTokens")))))))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            private MemberDeclarationSyntax CreateObjectComparerDeclaration(IEnumerable<ExpressionType> allTypes)
            {
                var types = (from r in allTypes
                             where r is EnumType || r is ObjectType || r is FunctionType || r is NumberRangeType
                             group r by r.StorageType into g
                             select new
                             {
                                 g.Key,
                                 Reference = this.Reference(g.Key),
                             }).ToList();

                var scope = new Scope<object>()
                    .Reserve("x")
                    .Reserve("y");
                scope = types.Aggregate(scope, (s, t) => s
                    .AddPrivate(("x", t.Key), $"x as {t.Key}")
                    .AddPrivate(("y", t.Key), $"y as {t.Key}"));

                return SyntaxFactory.ClassDeclaration(SyntaxHelper.Identifier(this.result.NamespaceScope.GetPublic("ObjectComparer")))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IComparer"))
                            .AddTypeArgumentListArguments(SyntaxHelper.ObjectType)))
                    .AddMembers(
                        SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxHelper.IdentifierName("ObjectComparer"))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier("Instance"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxHelper.IdentifierName("ObjectComparer"))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList())))))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                        SyntaxFactory.ConstructorDeclaration(
                            SyntaxFactory.Identifier("ObjectComparer"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                            .WithBody(
                                SyntaxFactory.Block()),
                        SyntaxFactory.MethodDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                            SyntaxFactory.Identifier("Compare"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("x"))
                                    .WithType(SyntaxHelper.ObjectType),
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("y"))
                                    .WithType(SyntaxHelper.ObjectType))
                            .WithBody(
                                SyntaxFactory.Block(
                                    types.Select(
                                        t =>
                                            SyntaxFactory.IfStatement(
                                                SyntaxFactory.IsPatternExpression(
                                                    SyntaxHelper.IdentifierName("x"),
                                                    SyntaxFactory.DeclarationPattern(
                                                        t.Reference,
                                                        SyntaxFactory.SingleVariableDesignation(
                                                            SyntaxHelper.Identifier(scope.GetPrivate(("x", t.Key)))))),
                                                SyntaxFactory.Block(
                                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                                        SyntaxFactory.IfStatement(
                                                            SyntaxFactory.IsPatternExpression(
                                                                SyntaxHelper.IdentifierName("y"),
                                                                SyntaxFactory.DeclarationPattern(
                                                                    t.Reference,
                                                                    SyntaxFactory.SingleVariableDesignation(
                                                                        SyntaxHelper.Identifier(scope.GetPrivate(("y", t.Key)))))),
                                                            SyntaxFactory.Block(
                                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                                    SyntaxFactory.ReturnStatement(
                                                                        SyntaxFactory.InvocationExpression(
                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                SyntaxHelper.IdentifierName(scope.GetPrivate(("x", t.Key))),
                                                                                SyntaxHelper.IdentifierName("CompareTo")))
                                                                            .WithArgumentList(
                                                                                SyntaxFactory.ArgumentList(
                                                                                    SyntaxFactory.SingletonSeparatedList(
                                                                                        SyntaxFactory.Argument(
                                                                                            SyntaxHelper.IdentifierName(scope.GetPrivate(("y", t.Key)))))))))))
                                                            .WithElse(
                                                                SyntaxFactory.ElseClause(
                                                                    SyntaxFactory.Block(
                                                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                                                            SyntaxFactory.ReturnStatement(
                                                                                SyntaxFactory.PrefixUnaryExpression(
                                                                                    SyntaxKind.UnaryMinusExpression,
                                                                                    SyntaxHelper.LiteralExpression(1))))))))))
                                                .WithElse(
                                                    SyntaxFactory.ElseClause(
                                                        SyntaxFactory.IfStatement(
                                                            SyntaxFactory.BinaryExpression(
                                                                SyntaxKind.IsExpression,
                                                                SyntaxHelper.IdentifierName("y"),
                                                                t.Reference),
                                                            SyntaxFactory.Block(
                                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                                    SyntaxFactory.ReturnStatement(
                                                                        SyntaxHelper.LiteralExpression(1)))))))).ToArray()))
                            .AddBodyStatements(
                                SyntaxFactory.ReturnStatement(
                                            SyntaxHelper.LiteralExpression(0))));
            }

            private FieldDeclarationSyntax CreateObjectDeclaration(ObjectInfo objectInfo, string value) =>
                SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        this.Reference(objectInfo.ReturnType),
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxHelper.Identifier(this.result.GameStateScope.GetPublic(objectInfo)))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                SyntaxHelper.LiteralExpression(value))))))
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.ConstKeyword));

            private ExpressionSyntax CreateObjectReference(ObjectInfo objectInfo)
            {
                switch (objectInfo.ReturnType)
                {
                    case EnumType enumType:
                        return SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this.Reference(enumType),
                            SyntaxHelper.IdentifierName(enumType.Scope.GetPublic(objectInfo)));

                    case NumberRangeType numberRangeType:
                        return SyntaxHelper.LiteralExpression((int)objectInfo.Value);

                    default:
                        return SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(objectInfo));
                }
            }

            private MemberDeclarationSyntax[] CreatePublicTypeDeclarations(IEnumerable<ExpressionType> renderedTypes)
            {
                var enumTypes = new List<EnumType>();
                var result = new List<MemberDeclarationSyntax>();
                foreach (var type in renderedTypes)
                {
                    switch (type)
                    {
                        case EnumType enumType:
                            enumTypes.Add(enumType);
                            result.Add(this.CreateEnumTypeDeclaration(enumType));
                            break;

                        case FunctionType functionType:
                            result.Add(this.CreateFunctionTypeDeclaration(functionType));
                            break;

                        case StateType stateType:
                            result.Add(this.CreateStateTypeDeclaration(stateType));
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                if (enumTypes.Count > 0)
                {
                    result.Add(this.CreateEnumLookupDeclaration(enumTypes));
                }

                return result.ToArray();
            }

            private MemberDeclarationSyntax[] CreateSharedGameStateDeclarations()
            {
                var moveIdentifier = SyntaxHelper.Identifier("move");
                var moveIdentifierName = SyntaxHelper.IdentifierName("move");

                return new MemberDeclarationSyntax[]
                {
                    SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxHelper.Identifier("IReadOnlyList"))
                            .AddTypeArgumentListArguments(
                                SyntaxHelper.IdentifierName("PlayerToken")),
                        SyntaxHelper.Identifier("Players"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxHelper.Identifier("IEnumerable"))
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.GenericName(
                                    SyntaxHelper.Identifier("IWeighted"))
                                    .AddTypeArgumentListArguments(
                                        SyntaxFactory.GenericName(
                                            SyntaxHelper.Identifier("IGameState"))
                                            .AddTypeArgumentListArguments(
                                                SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))))),
                        SyntaxHelper.Identifier("GetOutcomes"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithParameterList(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Parameter(
                                        moveIdentifier)
                                    .WithType(
                                        SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move"))))))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.YieldStatement(
                                        SyntaxKind.YieldReturnStatement,
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxHelper.IdentifierName("Weighted"),
                                                SyntaxHelper.IdentifierName("Create")))
                                            .AddArgumentListArguments(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxHelper.IdentifierName("MakeMove")))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                moveIdentifierName))),
                                                SyntaxFactory.Argument(
                                                    SyntaxHelper.LiteralExpression(1))))))),
                    SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxHelper.Identifier("IList"))
                            .AddTypeArgumentListArguments(
                                SyntaxHelper.ObjectType),
                        SyntaxHelper.Identifier("FormatTokens"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName("state")),
                                    SyntaxHelper.IdentifierName("FormatTokens"))))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxHelper.Identifier("IEnumerable"))
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.GenericName(
                                    SyntaxHelper.Identifier("IGameState"))
                                    .AddTypeArgumentListArguments(
                                        SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")))),
                        SyntaxHelper.Identifier("GetView"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxHelper.Identifier("playerToken"))
                                .WithType(
                                    SyntaxHelper.IdentifierName("PlayerToken")),
                            SyntaxFactory.Parameter(
                                SyntaxHelper.Identifier("maxStates"))
                            .WithType(
                                SyntaxHelper.IntType))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.YieldStatement(
                                        SyntaxKind.YieldReturnStatement,
                                        SyntaxFactory.ThisExpression())))),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxHelper.IntType,
                        SyntaxHelper.Identifier("CompareTo"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithParameterList(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Parameter(
                                        SyntaxHelper.Identifier("other"))
                                        .WithType(
                                            SyntaxFactory.GenericName(
                                                SyntaxHelper.Identifier("IGameState"))
                                                .AddTypeArgumentListArguments(
                                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("Move")))))))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.IfStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxHelper.ObjectType,
                                            SyntaxHelper.IdentifierName("ReferenceEquals")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxHelper.IdentifierName("other")),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.ThisExpression())),
                                    SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxHelper.LiteralExpression(0))))),
                                SyntaxFactory.LocalDeclarationStatement(
                                    SyntaxFactory.VariableDeclaration(
                                        SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier("state"))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.BinaryExpression(
                                                    SyntaxKind.AsExpression,
                                                    SyntaxHelper.IdentifierName("other"),
                                                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic("GameState"))))))),
                                SyntaxFactory.IfStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxHelper.ObjectType,
                                            SyntaxHelper.IdentifierName("ReferenceEquals")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxHelper.IdentifierName("state")),
                                            SyntaxFactory.Argument(
                                                SyntaxHelper.Null)),
                                    SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxHelper.LiteralExpression(1))))),
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxHelper.IdentifierName("state")),
                                            SyntaxHelper.IdentifierName("CompareTo")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxHelper.IdentifierName("state"),
                                                    SyntaxHelper.IdentifierName("state"))))))),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxHelper.StringType,
                        SyntaxHelper.Identifier("ToString"))
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxHelper.StringType,
                                        SyntaxHelper.IdentifierName("Concat")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxHelper.IdentifierName("FlattenFormatTokens")))))))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxHelper.IdentifierName("Task"),
                        SyntaxFactory.Identifier("ToXml"))
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("writer"))
                                .WithType(
                                    SyntaxHelper.IdentifierName("XmlWriter")))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxHelper.IdentifierName("state")),
                                        SyntaxHelper.IdentifierName("ToXml")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.IdentifierName("writer")))))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                };
            }

            private ClassDeclarationSyntax CreateStateTypeDeclaration(StateType stateType)
            {
                var types = (from r in stateType.Relations
                             group r by r.ReturnType.StorageType into g
                             select new
                             {
                                 g.Key,
                                 Reference = this.Reference(g.Key),
                             }).ToList();

                // TODO: Move to assign names pass.
                // TODO: Use the object name if there is only a single expression in a group.
                var fieldNames = types.Aggregate(new Scope<object>(), (s, r) => s.AddPrivate(r.Key, r.Key.ToString()));

                var classElement = SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.GetPublic(stateType))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxHelper.IdentifierName("ITokenFormattable")),
                        SyntaxFactory.SimpleBaseType(
                            SyntaxHelper.IdentifierName("IXml")));

                var constructor = SyntaxFactory.ConstructorDeclaration(classElement.Identifier)
                    .WithBody(SyntaxFactory.Block())
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var type in types)
                {
                    var fieldVariable = fieldNames.GetPrivate(type.Key);

                    classElement = classElement.AddMembers(
                        SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.GenericName(
                                    SyntaxHelper.Identifier("HashSet"),
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(type.Reference))))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxHelper.Identifier(fieldVariable))))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));

                    constructor = constructor
                        .AddBodyStatements(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName(fieldVariable)),
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.GenericName(
                                            SyntaxHelper.Identifier("HashSet"))
                                            .AddTypeArgumentListArguments(
                                                type.Reference))
                                        .WithArgumentList(SyntaxFactory.ArgumentList()))));
                }

                var formatTokensScope = fieldNames
                    .AddPrivate("tokens", "tokens");

                var formatTokens = SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxHelper.Identifier("IList"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))),
                    SyntaxHelper.Identifier("FormatTokens"))
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithBody(
                                    SyntaxFactory.Block()
                                        .AddStatements(
                                            SyntaxFactory.LocalDeclarationStatement(
                                                SyntaxFactory.VariableDeclaration(
                                                    SyntaxHelper.IdentifierName("var"))
                                                .AddVariables(
                                                    SyntaxFactory.VariableDeclarator(
                                                        SyntaxHelper.Identifier(formatTokensScope.GetPrivate("tokens")))
                                                    .WithInitializer(
                                                        SyntaxFactory.EqualsValueClause(
                                                            SyntaxFactory.ObjectCreationExpression(
                                                                SyntaxFactory.GenericName(
                                                                    SyntaxHelper.Identifier("List"))
                                                                .AddTypeArgumentListArguments(
                                                                    SyntaxFactory.PredefinedType(
                                                                        SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))
                                                            .WithArgumentList(
                                                                SyntaxFactory.ArgumentList()))))))
                                            .AddStatements(types.Select(obj =>
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxHelper.IdentifierName(formatTokensScope.GetPrivate("tokens")),
                                                            SyntaxHelper.IdentifierName("AddRange")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.InvocationExpression(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.ThisExpression(),
                                                                        SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key))),
                                                                    SyntaxHelper.IdentifierName("Select")))
                                                                .AddArgumentListArguments(
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.SimpleLambdaExpression(
                                                                            SyntaxFactory.Parameter(
                                                                                SyntaxHelper.Identifier("o")),
                                                                            SyntaxFactory.CastExpression(
                                                                                SyntaxHelper.ObjectType,
                                                                                SyntaxHelper.IdentifierName("o"))))))))).ToArray())
                                            .AddStatements(
                                                SyntaxFactory.ReturnStatement(
                                                    SyntaxHelper.IdentifierName(formatTokensScope.GetPrivate("tokens"))))))));

                var containsScope = fieldNames
                    .AddPrivate("value", "value");

                var contains = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxHelper.Identifier("Contains"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier(containsScope.GetPrivate("value")))
                            .WithType(
                                this.Reference(stateType)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SwitchStatement(
                                SyntaxHelper.IdentifierName(containsScope.GetPrivate("value")))
                                .WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                .WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken))
                                .AddSections(types.Select(obj =>
                                    SyntaxFactory.SwitchSection()
                                        .AddLabels(
                                            SyntaxFactory.CasePatternSwitchLabel(
                                                SyntaxFactory.DeclarationPattern(
                                                    obj.Reference,
                                                    SyntaxFactory.SingleVariableDesignation(
                                                        SyntaxHelper.Identifier(fieldNames.GetPrivate(obj.Key)))),
                                                SyntaxFactory.Token(SyntaxKind.ColonToken)))
                                        .AddStatements(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key))),
                                                        SyntaxHelper.IdentifierName("Contains")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key))))))).ToArray()),
                            SyntaxFactory.ReturnStatement(
                                SyntaxHelper.False)));

                var addScope = fieldNames
                    .AddPrivate("value", "value");

                var add = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxHelper.Identifier("Add"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier(addScope.GetPrivate("value")))
                            .WithType(
                                this.Reference(stateType)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SwitchStatement(
                                SyntaxHelper.IdentifierName(addScope.GetPrivate("value")))
                                .WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                .WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken))
                                .AddSections(types.Select(obj =>
                                    SyntaxFactory.SwitchSection()
                                        .AddLabels(
                                            SyntaxFactory.CasePatternSwitchLabel(
                                                SyntaxFactory.DeclarationPattern(
                                                    obj.Reference,
                                                    SyntaxFactory.SingleVariableDesignation(
                                                        SyntaxHelper.Identifier(fieldNames.GetPrivate(obj.Key)))),
                                                SyntaxFactory.Token(SyntaxKind.ColonToken)))
                                        .AddStatements(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key))),
                                                        SyntaxHelper.IdentifierName("Add")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key))))))).ToArray()),
                            SyntaxFactory.ReturnStatement(
                                SyntaxHelper.False)));

                var compareTo = SyntaxFactory.MethodDeclaration(
                    SyntaxHelper.IntType,
                    SyntaxHelper.Identifier("CompareTo"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(
                                    SyntaxHelper.Identifier("other"))
                                    .WithType(
                                        SyntaxHelper.IdentifierName("State"))))) // TODO: Name resolution.
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IntType)
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier("comp"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxHelper.LiteralExpression(0)))))));

                if (types.Count > 0)
                {
                    compareTo = compareTo
                        .AddBodyStatements(
                            SyntaxFactory.IfStatement(
                                types.Select(obj =>
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.NotEqualsExpression,
                                        SyntaxFactory.ParenthesizedExpression(
                                            SyntaxFactory.AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxHelper.IdentifierName("comp"),
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxHelper.IdentifierName("CompareUtilities"),
                                                        SyntaxHelper.IdentifierName("CompareSets")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ThisExpression(),
                                                                SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key)))),
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxHelper.IdentifierName("other"),
                                                                SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key))))))),
                                        SyntaxHelper.LiteralExpression(0)))
                                    .Aggregate((a, b) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, a, b)),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxHelper.IdentifierName("comp"))))));
                }

                compareTo = compareTo
                    .AddBodyStatements(
                        SyntaxFactory.ReturnStatement(
                            SyntaxHelper.IdentifierName("comp")));

                classElement = classElement.AddMembers(constructor, formatTokens, add, compareTo, contains);

                var toXmlBody = SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxHelper.IdentifierName("writer"),
                                    SyntaxHelper.IdentifierName("WriteStartElementAsync")))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NullLiteralExpression)),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("state"))),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NullLiteralExpression))))));

                foreach (var obj in types)
                {
                    toXmlBody = toXmlBody.AddStatements(
                        SyntaxFactory.ForEachStatement(
                            SyntaxHelper.IdentifierName("var"),
                            SyntaxFactory.Identifier("item"),
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxHelper.IdentifierName(fieldNames.GetPrivate(obj.Key))),
                            SyntaxFactory.Block(
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AwaitExpression(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxHelper.IdentifierName("writer"),
                                                SyntaxHelper.IdentifierName("WriteStartElementAsync")))
                                            .AddArgumentListArguments(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression)),
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal("fact"))),
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression))))),
                                this.CreateToXmlStatements(
                                    obj.Key,
                                    SyntaxFactory.IdentifierName("item"),
                                    SyntaxHelper.IdentifierName("writer")),
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AwaitExpression(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxHelper.IdentifierName("writer"),
                                                SyntaxHelper.IdentifierName("WriteEndElementAsync"))))))));
                }

                toXmlBody = toXmlBody.AddStatements(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxHelper.IdentifierName("writer"),
                                    SyntaxHelper.IdentifierName("WriteEndElementAsync"))))));

                classElement = classElement.AddMembers(
                    SyntaxFactory.MethodDeclaration(
                        SyntaxHelper.IdentifierName("Task"),
                        SyntaxFactory.Identifier("ToXml"))
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("writer"))
                                .WithType(
                                    SyntaxHelper.IdentifierName("XmlWriter")))
                        .WithBody(
                            toXmlBody));

                return SyntaxHelper.ReorderMembers(classElement);
            }

            private ExpressionStatementSyntax CreateToXmlStatements(ExpressionType type, ExpressionSyntax value, ExpressionSyntax writer)
            {
                switch (type)
                {
                    case FunctionType functionType:
                        return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        value,
                                        SyntaxHelper.IdentifierName("ToXml")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                writer))));

                    case ObjectType objectType:
                        return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        writer,
                                        SyntaxFactory.IdentifierName("WriteStringAsync")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.InvocationExpression(
                                                value)))));

                    case NumberRangeType numberRangeType:
                        return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        writer,
                                        SyntaxFactory.IdentifierName("WriteStringAsync")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    value,
                                                    SyntaxFactory.IdentifierName("ToString")))))));

                    case EnumType enumType:
                        return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        writer,
                                        SyntaxFactory.IdentifierName("WriteStringAsync")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.GetPublic("NameLookup")),
                                                    SyntaxFactory.IdentifierName("GetName")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(value))))));

                    case AnyType anyType:
                    case UnionType unionType:
                        break;
                }

                throw new NotSupportedException($"Could not enumerate the members of the type '{type}'");
            }

            private MemberDeclarationSyntax CreateTrueRelationDeclaration(RelationInfo @true) =>
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxHelper.Identifier(this.result.GameStateScope.GetPublic(@true)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier("value"))
                            .WithType(
                                this.Reference(@true.Arguments[0].ReturnType)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxHelper.IdentifierName("state")),
                                        SyntaxHelper.IdentifierName("Contains")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.IdentifierName("value"))))));

            private StatementSyntax FixVariables(Sentence sentence, Func<ExpressionScope, StatementSyntax> inner, ExpressionScope scope)
            {
                var variable = this.result.ContainedVariables[sentence].Where(v => !scope.Names.ContainsKey(scope.Variables[v])).FirstOrDefault();
                if (variable is object)
                {
                    var variableInfo = scope.Variables[variable];
                    var newScope = scope.Scope.AddPrivate(variableInfo, variable.Name);
                    scope = new ExpressionScope(scope.Variables, newScope, scope.Names.Add(variableInfo, SyntaxHelper.IdentifierName(newScope.GetPrivate(variableInfo))));

                    return SyntaxFactory.ForEachStatement(
                        this.Reference(variableInfo.ReturnType),
                        SyntaxHelper.Identifier(scope.Scope.GetPrivate(variableInfo)),
                        this.AllMembers(variableInfo.ReturnType, variableInfo.ReturnType, scope),
                        SyntaxFactory.Block(
                            this.FixVariables(sentence, inner, scope)));
                }
                else
                {
                    return inner(scope);
                }
            }

            private class ScopeWalker : SupportedExpressionsTreeWalker
            {
                private readonly Func<Term, ExpressionScope, ExpressionSyntax> convertExpression;
                private readonly ArgumentInfo[] parameters;
                private readonly CompileResult result;
                private readonly Runner runner;
                private ArgumentInfo param;
                private (string lexical, ExpressionSyntax expression, ExpressionType type) path;

                public ScopeWalker(CompileResult result, ArgumentInfo[] parameters, ExpressionScope scope, Runner runner, Func<Term, ExpressionScope, ExpressionSyntax> convertExpression)
                {
                    this.runner = runner;
                    this.result = result;
                    this.parameters = parameters;
                    this.convertExpression = convertExpression;
                    this.Declarations = new List<StatementSyntax>();
                    this.ParameterEquality = new List<ExpressionSyntax>();
                    this.ExpressionScope = scope;
                }

                public List<StatementSyntax> Declarations { get; }

                public ExpressionScope ExpressionScope { get; set; }

                public List<ExpressionSyntax> ParameterEquality { get; }

                public override void Walk(ConstantSentence constantSentence)
                {
                    Debug.Assert(this.parameters.Length == 0, "Arguments' arity doesn't match parameters' arity.");
                }

                public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
                {
                    var args = implicitRelationalSentence.Arguments;
                    Debug.Assert(this.parameters.Length == args.Count, "Arguments' arity doesn't match parameters' arity.");

                    for (var i = 0; i < args.Count; i++)
                    {
                        this.param = this.parameters[i];
                        var newPathString = this.ExpressionScope.Scope.GetPrivate(this.param);
                        this.path = (newPathString, SyntaxHelper.IdentifierName(newPathString), this.param.ReturnType);

                        var arg = implicitRelationalSentence.Arguments[i];
                        if (arg is IndividualVariable argVar)
                        {
                            var argInfo = this.result.AssignedTypes.GetExpressionInfo(arg, this.ExpressionScope.Variables);
                            var pStorage = this.param.ReturnType.StorageType;
                            var aStorage = argInfo.ReturnType.StorageType;

                            if (!ExpressionType.IsAssignableFrom(aStorage, pStorage))
                            {
                                var newArg = new VariableInfo(argVar.Id);
                                newArg.ReturnType = aStorage;
                                var newScope = this.ExpressionScope.Scope.AddPrivate(newArg, $"{arg} as {aStorage}");
                                var name = newScope.GetPrivate(newArg);

                                this.ExpressionScope = new ExpressionScope(
                                    this.ExpressionScope.Variables.SetItem(argVar, newArg),
                                    newScope,
                                    this.ExpressionScope.Names);

                                this.ParameterEquality.Add(
                                    SyntaxFactory.IsPatternExpression(
                                        this.path.expression,
                                        SyntaxFactory.DeclarationPattern(
                                            this.runner.Reference(aStorage),
                                            SyntaxFactory.SingleVariableDesignation(
                                                SyntaxHelper.Identifier(name)))));

                                this.path = (name, SyntaxHelper.IdentifierName(name), argInfo.ReturnType);
                            }
                        }

                        this.Walk((Expression)args[i]);
                    }
                }

                public override void Walk(Term term)
                {
                    if (term is IndividualVariable argVar)
                    {
                        var argVarInfo = this.ExpressionScope.Variables[argVar];
                        if (this.ExpressionScope.Names.ContainsKey(argVarInfo))
                        {
                            this.ParameterEquality.Add(SyntaxHelper.ObjectEqualsExpression(this.path.expression, this.ExpressionScope.Names[argVarInfo]));
                        }
                        else
                        {
                            this.ExpressionScope = new ExpressionScope(
                                this.ExpressionScope.Variables,
                                this.ExpressionScope.Scope.SetPrivate(argVarInfo, this.path.lexical),
                                this.ExpressionScope.Names.Add(argVarInfo, this.path.expression));
                        }
                    }
                    else
                    {
                        if (this.result.ContainedVariables[term].Count == 0)
                        {
                            this.ParameterEquality.Add(SyntaxHelper.ObjectEqualsExpression(this.path.expression, this.convertExpression(term, this.ExpressionScope)));
                        }
                        else
                        {
                            var arg = this.result.AssignedTypes.GetExpressionInfo(term, this.ExpressionScope.Variables);

                            // TODO: Allow arg to be a larger type than param. There's no iheritance. Will this matter? Perhaps with unions.
                            if (this.param.ReturnType != arg.ReturnType)
                            {
                                this.ExpressionScope = new ExpressionScope(
                                    this.ExpressionScope.Variables,
                                    this.ExpressionScope.Scope.AddPrivate(out var name, $"{this.path.expression} as {arg.ReturnType}"),
                                    this.ExpressionScope.Names);

                                this.ParameterEquality.Add(
                                    SyntaxFactory.IsPatternExpression(
                                        this.path.expression,
                                        SyntaxFactory.DeclarationPattern(
                                            this.runner.Reference(arg.ReturnType),
                                            SyntaxFactory.SingleVariableDesignation(
                                                SyntaxHelper.Identifier(name)))));

                                this.path = (name, SyntaxHelper.IdentifierName(name), arg.ReturnType);
                            }

                            base.Walk(term);
                        }
                    }
                }

                public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
                {
                    var args = implicitFunctionalTerm.Arguments;
                    var functionInfo = (FunctionInfo)this.result.AssignedTypes.GetExpressionInfo(implicitFunctionalTerm, this.ExpressionScope.Variables);

                    for (var i = 0; i < args.Count; i++)
                    {
                        var originalPath = this.path;

                        var param = functionInfo.Arguments[i];
                        var name = functionInfo.Scope.GetPublic(param);

                        var newPathString = this.path.lexical + "." + name;
                        this.path = (newPathString, SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this.path.expression,
                            SyntaxHelper.IdentifierName(name)), param.ReturnType);

                        var arg = implicitFunctionalTerm.Arguments[i];
                        if (arg is IndividualVariable argVar)
                        {
                            var argInfo = this.result.AssignedTypes.GetExpressionInfo(arg, this.ExpressionScope.Variables);
                            var pStorage = param.ReturnType.StorageType;
                            var aStorage = argInfo.ReturnType.StorageType;

                            if (!ExpressionType.IsAssignableFrom(aStorage, pStorage))
                            {
                                var newArg = new VariableInfo(argVar.Id);
                                newArg.ReturnType = aStorage;

                                this.ParameterEquality.Add(
                                    SyntaxFactory.IsPatternExpression(
                                        this.path.expression,
                                        SyntaxFactory.DeclarationPattern(
                                            this.runner.Reference(aStorage),
                                            SyntaxFactory.SingleVariableDesignation(
                                                SyntaxHelper.Identifier(name)))));

                                this.path = (name, SyntaxHelper.IdentifierName(name), argInfo.ReturnType);
                            }
                        }

                        this.Walk((Expression)args[i]);

                        this.path = originalPath;
                    }
                }
            }
        }
    }
}
