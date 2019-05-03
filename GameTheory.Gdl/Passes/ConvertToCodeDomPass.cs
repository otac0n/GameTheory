// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct ExpressionScope
    {
        public ExpressionScope(ImmutableDictionary<IndividualVariable, VariableInfo> variables, Scope<VariableInfo> scope, ImmutableDictionary<VariableInfo, ExpressionSyntax> names)
        {
            this.Variables = variables;
            this.Scope = scope;
            this.Names = names;
        }

        public ImmutableDictionary<IndividualVariable, VariableInfo> Variables { get; }

        public Scope<VariableInfo> Scope { get; }

        public ImmutableDictionary<VariableInfo, ExpressionSyntax> Names { get; }
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

            public static bool RequiresRuntimeCheck(ExpressionType t)
            {
                switch (t)
                {
                    case StateType stateType:
                    case UnionType unionType:
                    case IntersectionType intersectionType:
                    case NumberRangeType numberRangeType:
                        return true;

                    default:
                        return false;
                }
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

            public TypeSyntax Reference(ExpressionType type)
            {
                var builtIn = type.BuiltInType;
                if (builtIn != null)
                {
                    return ReferenceBuiltIn(builtIn);
                }

                switch (type)
                {
                    case EnumType enumType:
                    case FunctionType functionType:
                        var typeName = this.result.NamespaceScope.GetPublic(type);
                        var typeReference = SyntaxHelper.IdentifierName(typeName);
                        return this.result.GameStateScope.ContainsName(typeName)
                            ? SyntaxFactory.QualifiedName(SyntaxHelper.IdentifierName(this.result.GlobalScope.GetPublic(this.result)), typeReference)
                            : (TypeSyntax)typeReference;

                    case StateType stateType:
                        return SyntaxHelper.ObjectType;
                }

                throw new NotSupportedException($"Could not reference the type '{type}'");
            }

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

            public ExpressionSyntax AllMembers(ExpressionType type, ExpressionType declaredAs)
            {
                switch (type)
                {
                    case AnyType anyType:
                        // TODO: Need to return all ground expressions.
                        throw new NotImplementedException();

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
                        return SyntaxHelper.EnumerableRangeExpression(numberRangeType.Start, numberRangeType.End - numberRangeType.Start + 1);

                    case EnumType enumType:
                        return SyntaxFactory.ParenthesizedExpression(
                            SyntaxFactory.CastExpression(
                                SyntaxHelper.ArrayType(this.Reference(declaredAs)),
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

                    case UnionType unionType:
                        return unionType.Expressions
                            .Select(expr => this.AllMembers(expr.ReturnType, declaredAs))
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
                    t.BuiltInType == null &&
                    (!(t is ObjectType) || t is FunctionType) &&
                    (!RequiresRuntimeCheck(t) || t is StateType));
                var renderedExpressions = allExpressions.Except(init, next, does).Where(e =>
                    !(e is VariableInfo) &&
                    !(e is FunctionInfo) &&
                    !(e is ObjectInfo objectInfo && (objectInfo.Value is int || objectInfo.ReturnType is EnumType)));
                var renderedInMakeMove = renderedExpressions.ToLookup(e => this.result.MakeMoveScope.ContainsKey(e));

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
                            SyntaxHelper.IdentifierName("GameTheory")));

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
                            SyntaxHelper.IdentifierName("ITokenFormattable")))
                    .AddMembers(this.CreateGameStateConstructorDeclarations(init, stateType, role, moveType, noop))
                    .AddMembers(
                        this.CreateMakeMoveDeclaration(next, stateType, role, does, noop, renderedInMakeMove[true]),
                        this.CreateGetWinnersDeclaration(goal, role, terminal),
                        this.CreateGetAvailableMovesDeclaration(legal, role, terminal))
                    .AddMembers(this.CreateSharedGameStateDeclarations())
                    .AddMembers(
                        renderedInMakeMove[false].Select(expr =>
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

            private MemberDeclarationSyntax[] CreatePublicTypeDeclarations(IEnumerable<ExpressionType> renderedTypes) => renderedTypes.Select(type =>
            {
                switch (type)
                {
                    case EnumType enumType:
                        return (MemberDeclarationSyntax)this.CreateEnumTypeDeclaration(enumType);

                    case FunctionType functionType:
                        return (MemberDeclarationSyntax)this.CreateFunctionTypeDeclaration(functionType);

                    case StateType stateType:
                        return (MemberDeclarationSyntax)this.CreateStateTypeDeclaration(stateType);
                }

                throw new InvalidOperationException();
            }).ToArray();

            private MemberDeclarationSyntax CreateObjectComparerDeclaration(IEnumerable<ExpressionType> allTypes)
            {
                var types = (from r in allTypes
                             where r is EnumType || r is ObjectType || r is FunctionType || r is NumberRangeType
                             group r by (object)r.BuiltInType ?? r into g
                             select new
                             {
                                 g.Key,
                                 Reference = g.Key is ExpressionType ? this.Reference((ExpressionType)g.Key) : ReferenceBuiltIn((Type)g.Key),
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
                };
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

            private static ObjectCreationExpressionSyntax NewPlayerTokenExpression() =>
                SyntaxFactory.ObjectCreationExpression(SyntaxHelper.IdentifierName("PlayerToken"))
                    .WithArgumentList(SyntaxFactory.ArgumentList());

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
                                        SyntaxHelper.Identifier("moves"))))
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
                                SyntaxFactory.ExpressionStatement(
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
                                                this.ConvertExpression(((ImplicitRelationalSentence)i).Arguments[0], s1)))),
                            },
                            new Scope<VariableInfo>(),
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
                                SyntaxHelper.Identifier("moves"))
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
                                        SyntaxHelper.IdentifierName("moves")),
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
                                        SyntaxHelper.IdentifierName("moves")),
                                    SyntaxHelper.Coalesce(
                                        SyntaxHelper.IdentifierName("moves"),
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
                                        SyntaxHelper.IdentifierName("moves")),
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
                                        SyntaxHelper.IdentifierName("moves")),
                                    SyntaxHelper.Coalesce(
                                        SyntaxHelper.IdentifierName("moves"),
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

            private MethodDeclarationSyntax CreateGetAvailableMovesDeclaration(RelationInfo legal, RelationInfo role, LogicalInfo terminal)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;
                var statements = SyntaxFactory.Block(
                    this.ConvertImplicatedSentences(
                        legal.Body,
                        (i, s1) =>
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
                                                                    this.ConvertExpression(implicated.Arguments[0], s1))))),
                                                    SyntaxFactory.Argument(this.ConvertExpression(implicated.Arguments[1], s1))))));

                            return new[]
                            {
                                roles.Count > 1
                                    ? SyntaxFactory.IfStatement(
                                        SyntaxFactory.IsPatternExpression(
                                            SyntaxFactory.ElementAccessExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxHelper.IdentifierName("moves")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.CastExpression(
                                                            SyntaxHelper.IntType,
                                                            this.ConvertExpression(implicated.Arguments[0], s1)))),
                                            SyntaxFactory.ConstantPattern(
                                                SyntaxHelper.Null)),
                                        SyntaxFactory.Block(
                                            addStatement))
                                    : addStatement,
                            };
                        },
                        new Scope<VariableInfo>(),
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
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(terminal))))),
                                statements),
                            SyntaxFactory.ReturnStatement(
                            SyntaxHelper.IdentifierName("moves"))));
            }

            private MethodDeclarationSyntax CreateGetWinnersDeclaration(RelationInfo goal, RelationInfo role, LogicalInfo terminal)
            {
                return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxHelper.Identifier("IReadOnlyCollection"))
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
                                            SyntaxHelper.Identifier("winners"))
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
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(terminal)))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ForStatement(
                                            SyntaxFactory.Block(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxHelper.IdentifierName("winners"),
                                                            SyntaxHelper.IdentifierName("AddRange")))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.InvocationExpression(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.InvocationExpression(
                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                this.AllMembers(role.Arguments[0].ReturnType, role.Arguments[0].ReturnType),
                                                                                SyntaxHelper.IdentifierName("Where")))
                                                                            .AddArgumentListArguments(
                                                                                SyntaxFactory.Argument(
                                                                                    SyntaxFactory.SimpleLambdaExpression(
                                                                                        SyntaxFactory.Parameter(
                                                                                            SyntaxHelper.Identifier("role")),
                                                                                        SyntaxFactory.InvocationExpression(
                                                                                            SyntaxFactory.MemberAccessExpression(
                                                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                                                SyntaxFactory.ThisExpression(),
                                                                                                SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(goal))))
                                                                                            .AddArgumentListArguments(
                                                                                                SyntaxFactory.Argument(
                                                                                                    SyntaxHelper.IdentifierName("role")),
                                                                                                SyntaxFactory.Argument(
                                                                                                    SyntaxHelper.IdentifierName("g")))))),
                                                                        SyntaxHelper.IdentifierName("Select")))
                                                                        .AddArgumentListArguments(
                                                                            SyntaxFactory.Argument(
                                                                                SyntaxFactory.SimpleLambdaExpression(
                                                                                    SyntaxFactory.Parameter(
                                                                                        SyntaxHelper.Identifier("role")),
                                                                                    SyntaxFactory.ElementAccessExpression(
                                                                                        SyntaxFactory.MemberAccessExpression(
                                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                                            SyntaxFactory.ThisExpression(),
                                                                                            SyntaxHelper.IdentifierName("Players")))
                                                                                        .AddArgumentListArguments(
                                                                                            SyntaxFactory.Argument(
                                                                                                SyntaxFactory.CastExpression(
                                                                                                    SyntaxHelper.IntType,
                                                                                                    SyntaxHelper.IdentifierName("role")))))))))),
                                                SyntaxFactory.IfStatement(
                                                    SyntaxFactory.BinaryExpression(
                                                        SyntaxKind.GreaterThanExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxHelper.IdentifierName("winners"),
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
                                                                SyntaxHelper.Identifier("g"))
                                                                .WithInitializer(
                                                                    SyntaxFactory.EqualsValueClause(
                                                                        SyntaxHelper.LiteralExpression(100))))))
                                            .WithCondition(
                                                SyntaxFactory.BinaryExpression(
                                                    SyntaxKind.GreaterThanOrEqualExpression,
                                                    SyntaxHelper.IdentifierName("g"),
                                                    SyntaxHelper.LiteralExpression(1)))
                                            .WithIncrementors(
                                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                    SyntaxFactory.PostfixUnaryExpression(
                                                        SyntaxKind.PostDecrementExpression,
                                                        SyntaxHelper.IdentifierName("g"))))))),
                            SyntaxFactory.ReturnStatement(
                                SyntaxHelper.IdentifierName("winners"))));
            }

            private MethodDeclarationSyntax CreateMakeMoveDeclaration(RelationInfo next, ExpressionType stateType, RelationInfo role, RelationInfo does, ObjectInfo noop, IEnumerable<ExpressionInfo> internalRelations)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;

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
                                    SyntaxHelper.Identifier(this.result.MakeMoveScope.GetPrivate("move")))
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
                                            SyntaxHelper.Identifier(this.result.MakeMoveScope.GetPrivate("role")))
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
                                                                SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("move")),
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
                                                SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("moves"))))
                                            .WithArgumentList(
                                                SyntaxFactory.BracketedArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                        SyntaxFactory.Argument(
                                                            SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("role")))))),
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
                                                            SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("move")))))))),
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
                                                                    SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("move")))))))))),
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxHelper.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxHelper.Identifier(this.result.MakeMoveScope.GetPrivate("moves")))
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
                                                SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("moves")))),
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("moves"))),
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.LiteralExpression(roles.Count)))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.ElementAccessExpression(
                                        SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("moves")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("role")))),
                                    SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("move")))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("moves")),
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
                                                        SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("moves")))))))),
                            SyntaxFactory.LocalFunctionStatement(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                                SyntaxHelper.Identifier(this.result.MakeMoveScope.GetPublic(does)))
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(
                                        SyntaxHelper.Identifier("r"))
                                        .WithType(
                                            this.Reference(does.Arguments[0].ReturnType)),
                                    SyntaxFactory.Parameter(
                                        SyntaxHelper.Identifier("m"))
                                        .WithType(
                                            this.Reference(does.Arguments[1].ReturnType)))
                            .WithExpressionBody(
                                SyntaxFactory.ArrowExpressionClause(
                                    SyntaxHelper.ObjectEqualsExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ElementAccessExpression(
                                                SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("moves")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.CastExpression(
                                                            SyntaxHelper.IntType,
                                                            SyntaxHelper.IdentifierName("r")))),
                                            SyntaxHelper.IdentifierName("Value")),
                                        SyntaxHelper.IdentifierName("m"))))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
                else
                {
                    makeMove = makeMove
                        .AddBodyStatements(
                            SyntaxFactory.LocalFunctionStatement(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                                SyntaxHelper.Identifier(this.result.MakeMoveScope.GetPublic(does)))
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(
                                        SyntaxHelper.Identifier("r"))
                                        .WithType(
                                            this.Reference(does.Arguments[0].ReturnType)),
                                    SyntaxFactory.Parameter(
                                        SyntaxHelper.Identifier("m"))
                                        .WithType(
                                            this.Reference(does.Arguments[1].ReturnType)))
                                .WithExpressionBody(
                                    SyntaxFactory.ArrowExpressionClause(
                                        SyntaxHelper.ObjectEqualsExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxHelper.IdentifierName(this.result.MakeMoveScope.GetPrivate("move")),
                                                SyntaxHelper.IdentifierName("Value")),
                                            SyntaxHelper.IdentifierName("m"))))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }

                makeMove = makeMove
                    .AddBodyStatements(
                        internalRelations.Select(expr =>
                        {
                            MethodDeclarationSyntax function;
                            switch (expr)
                            {
                                case RelationInfo relationInfo:
                                    function = this.CreateLogicalFunctionDeclaration(relationInfo, relationInfo.Arguments, relationInfo.Body);
                                    break;

                                case LogicalInfo logicalInfo:
                                    function = this.CreateLogicalFunctionDeclaration(logicalInfo, Array.Empty<ArgumentInfo>(), logicalInfo.Body);
                                    break;

                                default:
                                    throw new InvalidOperationException();
                            }

                            return SyntaxFactory.LocalFunctionStatement(function.ReturnType, function.Identifier)
                                .WithParameterList(function.ParameterList)
                                .WithBody(function.Body)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                        }).ToArray());

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
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            nextIdentifierName,
                                            SyntaxHelper.IdentifierName("Add")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                this.ConvertExpression(((ImplicitRelationalSentence)i).Arguments[0], s1)))),
                            },
                            this.result.MakeMoveScope.SubScope<VariableInfo>(),
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

            private MethodDeclarationSyntax CreateLogicalFunctionDeclaration(ExpressionInfo expression, ArgumentInfo[] parameters, IEnumerable<Sentence> sentences)
            {
                var nameScope = (expression as RelationInfo)?.Scope ?? new Scope<VariableInfo>();

                var methodElement = SyntaxFactory.MethodDeclaration(
                    this.Reference(expression.ReturnType),
                    this.result.GameStateScope.TryGetPublic(expression) ?? this.result.MakeMoveScope.GetPublic(expression))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                foreach (var param in parameters)
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxHelper.Identifier(nameScope.GetPrivate(param)))
                        .WithType(this.Reference(param.ReturnType)));
                }

                var returnTrue = SyntaxFactory.ReturnStatement(SyntaxHelper.True);

                foreach (var sentence in sentences)
                {
                    var implicated = sentence.GetImplicatedSentence();

                    var walker = new ScopeWalker(
                        this.result,
                        parameters,
                        new ExpressionScope(this.result.AssignedTypes.VariableTypes[sentence], nameScope, ImmutableDictionary<VariableInfo, ExpressionSyntax>.Empty),
                        this.Reference,
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

                // TODO: Runtime type checks
                methodElement = methodElement.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxHelper.False));

                return methodElement;
            }

            private StatementSyntax[] ConvertImplicatedSentences(IEnumerable<Sentence> sentences, Func<Sentence, ExpressionScope, StatementSyntax[]> getImplication, Scope<VariableInfo> scope, ImmutableDictionary<VariableInfo, ExpressionSyntax> names)
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
                                    new ExpressionScope(null, null, null))); // TODO: Allow grouping by variables.
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

            private StatementSyntax ConvertSentence(Sentence sentence, Func<ExpressionScope, StatementSyntax[]> inner, ExpressionScope scope)
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
                        this.AllMembers(variableInfo.ReturnType, variableInfo.ReturnType),
                        SyntaxFactory.Block(
                            this.ConvertSentence(sentence, inner, scope)));
                }
                else
                {
                    return SyntaxFactory.IfStatement(
                        this.ConvertCondition(sentence, scope),
                        SyntaxFactory.Block(inner(scope)))
                    .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}")));
                }
            }

            private ExpressionSyntax ConvertCondition(Sentence condition, ExpressionScope scope)
            {
                switch (condition)
                {
                    case ImplicitRelationalSentence implicitRelationalSentence:
                        return this.ConvertImplicitRelationalCondition(implicitRelationalSentence, scope);

                    case ConstantSentence constantSentence:
                        return this.ConvertLogicalCondition(constantSentence);

                    case Negation negation:
                        return this.ConvertNegationCondition(negation, scope);

                    case Disjunction disjunction:
                        return this.ConvertDisjunctionCondition(disjunction, scope);

                    default:
                        throw new NotSupportedException($"Could not convert condition '{condition}'");
                }
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

            private ExpressionSyntax ConvertConstantExpression(Constant constant) =>
                this.CreateObjectReference((ObjectInfo)this.result.AssignedTypes.ExpressionTypes[(constant, 0)]);

            private ExpressionSyntax ConvertVariableExpression(IndividualVariable individualVariable, ExpressionScope scope) =>
                scope.Names[scope.Variables[individualVariable]];

            private ExpressionSyntax ConvertFunctionalTermExpression(ImplicitFunctionalTerm implicitFunctionalTerm, ExpressionScope scope) =>
                SyntaxFactory.ObjectCreationExpression( // TODO: Runtime type checks
                    SyntaxHelper.IdentifierName(this.result.NamespaceScope.GetPublic(this.result.AssignedTypes.GetExpressionInfo(implicitFunctionalTerm).ReturnType)))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitFunctionalTerm.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));

            private ExpressionSyntax ConvertLogicalCondition(ConstantSentence constantSentence)
            {
                var expressionInfo = this.result.AssignedTypes.GetExpressionInfo(constantSentence);
                var makeMoveLocalName = this.result.MakeMoveScope.TryGetPublic(expressionInfo);

                return SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    makeMoveLocalName != null
                        ? (ExpressionSyntax)SyntaxHelper.IdentifierName(makeMoveLocalName)
                        : SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(),
                            SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(expressionInfo))));
            }

            private ExpressionSyntax ConvertNegationCondition(Negation negation, ExpressionScope scope) =>
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    this.ConvertCondition(negation.Negated, scope));

            private ExpressionSyntax ConvertDisjunctionCondition(Disjunction disjunction, ExpressionScope scope) =>
                disjunction.Disjuncts.Select(d => this.ConvertCondition(d, scope)).Aggregate((a, b) =>
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.LogicalOrExpression,
                        a,
                        b));

            private ExpressionSyntax ConvertImplicitRelationalCondition(ImplicitRelationalSentence implicitRelationalSentence, ExpressionScope scope)
            {
                // TODO: Runtime type checks
                var expressionInfo = this.result.AssignedTypes.GetExpressionInfo(implicitRelationalSentence);
                var makeMoveLocalName = this.result.MakeMoveScope.TryGetPublic(expressionInfo);
                return SyntaxFactory.InvocationExpression(
                        makeMoveLocalName != null
                            ? (ExpressionSyntax)SyntaxHelper.IdentifierName(makeMoveLocalName)
                            : SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxHelper.IdentifierName(this.result.GameStateScope.GetPublic(expressionInfo))),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>()
                                .AddRange(implicitRelationalSentence.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));
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
                                                arg.ReturnType.BuiltInType == typeof(object)
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

                return SyntaxHelper.ReorderMembers(structElement);
            }

            private ClassDeclarationSyntax CreateStateTypeDeclaration(StateType stateType)
            {
                var types = (from r in stateType.Relations
                             group r by (object)r.ReturnType.BuiltInType ?? r.ReturnType into g
                             select new
                             {
                                 g.Key,
                                 Reference = g.Key is ExpressionType ? this.Reference((ExpressionType)g.Key) : ReferenceBuiltIn((Type)g.Key),
                             }).ToList();

                // TODO: Move to assign names pass.
                // TODO: Use the object name if there is only a single expression in a group.
                var fieldNames = types.Aggregate(new Scope<object>(), (s, r) => s.AddPrivate(r.Key, r.Key.ToString()));

                var classElement = SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.GetPublic(stateType))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxHelper.IdentifierName("ITokenFormattable")));

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
                                                        SyntaxHelper.Identifier("tokens"))
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
                                                            SyntaxHelper.IdentifierName("tokens"),
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
                                                    SyntaxHelper.IdentifierName("tokens")))))));

                var contains = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxHelper.Identifier("Contains"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier("value"))
                            .WithType(
                                this.Reference(stateType)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SwitchStatement(
                                SyntaxHelper.IdentifierName("value"))
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

                var add = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxHelper.Identifier("Add"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxHelper.Identifier("value"))
                            .WithType(
                                this.Reference(stateType)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SwitchStatement(
                                SyntaxHelper.IdentifierName("value"))
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

                return SyntaxHelper.ReorderMembers(classElement);
            }

            private class ScopeWalker : SupportedExpressionsTreeWalker
            {
                private readonly CompileResult result;
                private readonly ArgumentInfo[] parameters;
                private readonly Func<ExpressionType, TypeSyntax> reference;
                private readonly Func<Term, ExpressionScope, ExpressionSyntax> convertExpression;
                private ArgumentInfo param;
                private string pathString;
                private ExpressionSyntax path;

                public ScopeWalker(CompileResult result, ArgumentInfo[] parameters, ExpressionScope scope, Func<ExpressionType, TypeSyntax> reference, Func<Term, ExpressionScope, ExpressionSyntax> convertExpression)
                {
                    this.result = result;
                    this.parameters = parameters;
                    this.reference = reference;
                    this.convertExpression = convertExpression;
                    this.Declarations = new List<StatementSyntax>();
                    this.ParameterEquality = new List<ExpressionSyntax>();
                    this.ExpressionScope = scope;
                }

                public List<StatementSyntax> Declarations { get; }

                public List<ExpressionSyntax> ParameterEquality { get; }

                public ExpressionScope ExpressionScope { get; set; }

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
                        this.pathString = this.ExpressionScope.Scope.GetPrivate(this.param);
                        this.path = SyntaxHelper.IdentifierName(this.pathString);
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
                            this.ParameterEquality.Add(SyntaxHelper.ObjectEqualsExpression(this.path, this.ExpressionScope.Names[argVarInfo]));
                        }
                        else
                        {
                            this.ExpressionScope = new ExpressionScope(
                                this.ExpressionScope.Variables,
                                this.ExpressionScope.Scope.SetPrivate(argVarInfo, this.pathString),
                                this.ExpressionScope.Names.Add(argVarInfo, this.path));
                        }
                    }
                    else
                    {
                        if (this.result.ContainedVariables[term].Count == 0)
                        {
                            this.ParameterEquality.Add(SyntaxHelper.ObjectEqualsExpression(this.path, this.convertExpression(term, this.ExpressionScope)));
                        }
                        else
                        {
                            var arg = this.result.AssignedTypes.GetExpressionInfo(term);

                            // TODO: Allow arg to be a larger type than param. There's no iheritance. Will this matter? Perhaps with unions.
                            if (this.param.ReturnType != arg.ReturnType)
                            {
                                this.ExpressionScope = new ExpressionScope(
                                    this.ExpressionScope.Variables,
                                    this.ExpressionScope.Scope.AddPrivate(out var name, $"{this.path} as {arg.ReturnType}"),
                                    this.ExpressionScope.Names);

                                this.ParameterEquality.Add(
                                    SyntaxFactory.IsPatternExpression(
                                        this.path,
                                        SyntaxFactory.DeclarationPattern(
                                            this.reference(arg.ReturnType),
                                            SyntaxFactory.SingleVariableDesignation(
                                                SyntaxHelper.Identifier(name)))));

                                this.pathString = name;
                                this.path = SyntaxHelper.IdentifierName(this.pathString);
                            }

                            base.Walk(term);
                        }
                    }
                }

                public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
                {
                    var args = implicitFunctionalTerm.Arguments;
                    var functionInfo = (FunctionInfo)this.result.AssignedTypes.GetExpressionInfo(implicitFunctionalTerm);

                    for (var i = 0; i < args.Count; i++)
                    {
                        var originalPath = this.path;
                        var name = functionInfo.Scope.GetPublic(functionInfo.Arguments[i]);

                        this.pathString += "." + name;
                        this.path = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this.path,
                            SyntaxHelper.IdentifierName(name));

                        this.Walk((Expression)args[i]);

                        this.path = originalPath;
                    }
                }
            }
        }
    }
}
