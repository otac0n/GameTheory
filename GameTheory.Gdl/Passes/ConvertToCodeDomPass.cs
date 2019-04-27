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

            public TypeSyntax Reference(ExpressionType type)
            {
                switch (type)
                {
                    case AnyType anyType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                    case NoneType noneType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

                    case BooleanType booleanType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));

                    case ObjectType objectType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));

                    case NumberRangeType numberRangeType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

                    case EnumType enumType:
                    case FunctionType functionType:
                        var typeName = this.result.NamespaceScope.TryGetPublic(type);
                        var typeReference = SyntaxFactory.IdentifierName(typeName);
                        return this.result.GameStateScope.ContainsName(typeName)
                            ? SyntaxFactory.QualifiedName(SyntaxFactory.ParseName(this.result.GlobalScope.TryGetPublic(this.result)), typeReference)
                            : (TypeSyntax)typeReference;

                    case StateType stateType:
                    case UnionType unionType:
                    case IntersectionType intersectionType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                    case ExpressionType _ when type.BuiltInType != null:
                        return SyntaxFactory.ParseTypeName(type.BuiltInType.FullName);
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
                            SyntaxFactory.IdentifierName(enumType.Scope.TryGetPublic(objectInfo)));

                    case NumberRangeType numberRangeType:
                        return SyntaxHelper.LiteralExpression((int)objectInfo.Value);

                    default:
                        return SyntaxFactory.IdentifierName(this.result.GameStateScope.TryGetPublic(objectInfo));
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
                                    this.CreateObjectReference((ObjectInfo)this.result.AssignedTypes.ExpressionTypes[(objectType.Constant, 0)]))));

                    case NumberRangeType numberRangeType:
                        return SyntaxHelper.EnumerableRangeExpression(numberRangeType.Start, numberRangeType.End - numberRangeType.Start + 1);

                    case EnumType enumType:
                        return SyntaxFactory.ParenthesizedExpression(
                            SyntaxFactory.CastExpression(
                                SyntaxHelper.ArrayType(this.Reference(declaredAs)),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("Enum"),
                                        SyntaxFactory.IdentifierName("GetValues")),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.TypeOfExpression(
                                                    SyntaxFactory.ParseTypeName(this.result.NamespaceScope.TryGetPublic(enumType)))))))));

                    case UnionType unionType:
                        return unionType.Expressions
                            .Select(expr => this.AllMembers(expr.ReturnType, declaredAs))
                            .Aggregate((a, b) =>
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        a,
                                        SyntaxFactory.IdentifierName("Concat")))
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

                var root = SyntaxFactory.CompilationUnit();

                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(this.result.Name))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.IdentifierName("System")),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxFactory.IdentifierName("System"),
                                    SyntaxFactory.IdentifierName("Collections")),
                                SyntaxFactory.IdentifierName("Generic"))),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.IdentifierName("System"),
                                SyntaxFactory.IdentifierName("Linq"))),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.IdentifierName("GameTheory")));

                var move = this.CreateMoveTypeDeclaration(does.Arguments[1].ReturnType);

                var gameState = SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.TryGetPublic("GameState"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IGameState"))
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")))))
                    .AddMembers(this.CreateGameStateConstructorDeclarations(init, stateType, role, moveType, noop))
                    .AddMembers(
                        this.CreateMakeMoveDeclaration(next, stateType, role, does, noop),
                        this.CreateGetWinnersDeclaration(goal, terminal),
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
                                    else
                                    {
                                        return this.CreateLogicalFunctionDeclaration(relationInfo, relationInfo.Arguments, relationInfo.Body);
                                    }

                                case LogicalInfo logicalInfo:
                                    return this.CreateLogicalFunctionDeclaration(logicalInfo, Array.Empty<ArgumentInfo>(), logicalInfo.Body);
                            }

                            throw new InvalidOperationException();
                        }).ToArray());

                ns = ns
                    .AddMembers(gameState, move)
                    .AddMembers(
                        this.CreatePublicTypeDeclarations(renderedTypes));
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

            private MemberDeclarationSyntax[] CreateSharedGameStateDeclarations()
            {
                var moveIdentifier = SyntaxFactory.Identifier("move");
                var moveIdentifierName = SyntaxFactory.IdentifierName("move");

                return new MemberDeclarationSyntax[]
                {
                    SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("IReadOnlyList"))
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.IdentifierName("PlayerToken")),
                        SyntaxFactory.Identifier("Players"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.SetAccessorDeclaration)
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("IEnumerable"))
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("IWeighted"))
                                    .AddTypeArgumentListArguments(
                                        SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("IGameState"))
                                            .AddTypeArgumentListArguments(
                                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))))),
                        SyntaxFactory.Identifier("GetOutcomes"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithParameterList(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Parameter(
                                        moveIdentifier)
                                    .WithType(
                                        SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))))))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.YieldStatement(
                                        SyntaxKind.YieldReturnStatement,
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("Weighted"),
                                                SyntaxFactory.IdentifierName("Create")))
                                            .AddArgumentListArguments(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName("MakeMove")))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                moveIdentifierName))),
                                                SyntaxFactory.Argument(
                                                    SyntaxHelper.LiteralExpression(1))))))),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("IEnumerable"))
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("IGameState"))
                                    .AddTypeArgumentListArguments(
                                        SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")))),
                        SyntaxFactory.Identifier("GetView"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("playerToken"))
                                .WithType(
                                    SyntaxFactory.IdentifierName("PlayerToken")),
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("maxStates"))
                            .WithType(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.IntKeyword))))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.YieldStatement(
                                        SyntaxKind.YieldReturnStatement,
                                        SyntaxFactory.ThisExpression())))),
                    SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.PredefinedType(
                            SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                        SyntaxFactory.Identifier("CompareTo"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithParameterList(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Parameter(
                                        SyntaxFactory.Identifier("other"))
                                        .WithType(
                                            SyntaxFactory.GenericName(
                                                SyntaxFactory.Identifier("IGameState"))
                                                .AddTypeArgumentListArguments(
                                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")))))))
                        .WithBody(
                            SyntaxFactory.Block(
                                SyntaxFactory.IfStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                                            SyntaxFactory.IdentifierName("ReferenceEquals")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName("other")),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.ThisExpression())),
                                    SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList<StatementSyntax>(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxHelper.LiteralExpression(0))))),
                                SyntaxFactory.LocalDeclarationStatement(
                                    SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier("state"))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.BinaryExpression(
                                                    SyntaxKind.AsExpression,
                                                    SyntaxFactory.IdentifierName("other"),
                                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("GameState"))))))),
                                SyntaxFactory.IfStatement(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                                            SyntaxFactory.IdentifierName("ReferenceEquals")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName("state")),
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
                                                SyntaxFactory.IdentifierName("state")),
                                            SyntaxFactory.IdentifierName("CompareTo")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName("state"),
                                                    SyntaxFactory.IdentifierName("state"))))))),
                };
            }

            private ClassDeclarationSyntax CreateMoveTypeDeclaration(ExpressionType moveType) =>
                SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.TryGetPublic("Move"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxFactory.IdentifierName("IMove")))))
                    .AddMembers(
                        SyntaxFactory.ConstructorDeclaration(
                            SyntaxFactory.Identifier(this.result.NamespaceScope.TryGetPublic("Move")))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("playerToken"))
                                    .WithType(SyntaxFactory.IdentifierName("PlayerToken")),
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                                    .WithType(this.Reference(moveType)))
                            .WithBody(
                                SyntaxFactory.Block(
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName("PlayerToken")),
                                            SyntaxFactory.IdentifierName("playerToken"))),
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName("Value")),
                                            SyntaxFactory.IdentifierName("value"))))),
                        SyntaxFactory.PropertyDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                            SyntaxFactory.Identifier("IsDeterministic"))
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
                            SyntaxFactory.IdentifierName("PlayerToken"),
                            SyntaxFactory.Identifier("PlayerToken"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))),
                        SyntaxFactory.PropertyDeclaration(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IList"))
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))),
                            SyntaxFactory.Identifier("FormatTokens"))
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
                                                        SyntaxFactory.PredefinedType(
                                                            SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))
                                                    .WithInitializer(
                                                        SyntaxFactory.InitializerExpression(
                                                            SyntaxKind.ArrayInitializerExpression,
                                                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.ThisExpression(),
                                                                    SyntaxFactory.IdentifierName("Value")))))))))),
                        SyntaxFactory.PropertyDeclaration(
                            this.Reference(moveType),
                            SyntaxFactory.Identifier("Value"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))),
                        SyntaxFactory.MethodDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                            SyntaxFactory.Identifier("Equals"))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("obj"))
                                    .WithType(SyntaxHelper.ObjectType))
                            .WithExpressionBody(
                                SyntaxFactory.ArrowExpressionClause(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.LogicalAndExpression,
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.LogicalAndExpression,
                                            SyntaxFactory.IsPatternExpression(
                                                SyntaxFactory.IdentifierName("obj"),
                                                SyntaxFactory.DeclarationPattern(
                                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")),
                                                    SyntaxFactory.SingleVariableDesignation(
                                                        SyntaxFactory.Identifier("other")))),
                                            SyntaxHelper.ObjectEqualsExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxFactory.IdentifierName("PlayerToken")),
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName("other"),
                                                    SyntaxFactory.IdentifierName("PlayerToken")))),
                                        SyntaxHelper.ObjectEqualsExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName("Value")),
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("other"),
                                                SyntaxFactory.IdentifierName("Value"))))))
                            .WithSemicolonToken(
                                SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.MethodDeclaration(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                            SyntaxFactory.Identifier("ToString"))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                            .WithExpressionBody(
                                SyntaxFactory.ArrowExpressionClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                            SyntaxFactory.IdentifierName("Concat")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.ThisExpression(),
                                                        SyntaxFactory.IdentifierName("FlattenFormatTokens")))))))
                            .WithSemicolonToken(
                                SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            private static ObjectCreationExpressionSyntax NewPlayerTokenExpression() =>
                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("PlayerToken"))
                    .WithArgumentList(SyntaxFactory.ArgumentList());

            private FieldDeclarationSyntax CreateObjectDeclaration(ObjectInfo objectInfo, string value) =>
                SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        this.Reference(objectInfo.ReturnType),
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(this.result.GameStateScope.TryGetPublic(objectInfo)))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                SyntaxHelper.LiteralExpression(value))))))
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            private MemberDeclarationSyntax[] CreateGameStateConstructorDeclarations(RelationInfo init, StateType stateType, RelationInfo role, ExpressionType moveType, ObjectInfo noop)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;

                var declarations = new List<MemberDeclarationSyntax>
                {
                    SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.ParseTypeName(this.result.NamespaceScope.TryGetPublic(stateType)))
                            .AddVariables(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("state"))))
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
                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier("moves"))))
                            .AddModifiers(
                                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
                }

                var constructor1 = SyntaxFactory.ConstructorDeclaration(
                    SyntaxFactory.Identifier(this.result.NamespaceScope.TryGetPublic("GameState")))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("Players")),
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
                                        SyntaxFactory.IdentifierName("state")),
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.ParseTypeName(this.result.NamespaceScope.TryGetPublic(stateType)))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList())))));

                var scope = new Scope<VariableInfo>();
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
                                                SyntaxFactory.IdentifierName("state")),
                                            SyntaxFactory.IdentifierName("Add")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                this.ConvertExpression(((ImplicitRelationalSentence)i).Arguments[0], s1)))),
                            },
                            scope));

                var constructor2 = SyntaxFactory.ConstructorDeclaration(
                    SyntaxFactory.Identifier(this.result.NamespaceScope.TryGetPublic("GameState")))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("players"))
                            .WithType(
                                SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("IReadOnlyList"))
                                    .AddTypeArgumentListArguments(
                                        SyntaxFactory.IdentifierName("PlayerToken"))),
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("state"))
                            .WithType(
                                SyntaxFactory.ParseTypeName(this.result.NamespaceScope.TryGetPublic(stateType))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("Players")),
                                    SyntaxFactory.IdentifierName("players"))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("state")),
                                    SyntaxFactory.IdentifierName("state")))));

                if (roles.Count > 1)
                {
                    constructor2 = constructor2
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("moves"))
                                .WithType(
                                    SyntaxHelper.ArrayType(
                                        SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")))));

                    if (noop != null)
                    {
                        constructor1 = constructor1
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("moves")),
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName("FindForcedNoOps")))));

                        constructor2 = constructor2
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("moves")),
                                    SyntaxHelper.Coalesce(
                                        SyntaxFactory.IdentifierName("moves"),
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName("FindForcedNoOps"))))));
                    }
                    else
                    {
                        constructor1 = constructor1
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("moves")),
                                    SyntaxFactory.ArrayCreationExpression(
                                        SyntaxHelper.ArrayType(
                                            SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")),
                                            SyntaxHelper.LiteralExpression(roles.Count)))));

                        constructor2 = constructor2
                            .AddBodyStatements(
                                SyntaxHelper.AssignmentStatement(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("moves")),
                                    SyntaxHelper.Coalesce(
                                        SyntaxFactory.IdentifierName("moves"),
                                        SyntaxFactory.ArrayCreationExpression(
                                            SyntaxHelper.ArrayType(
                                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")),
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
                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))),
                            SyntaxFactory.Identifier("FindForcedNoOps"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                            .WithBody(
                                SyntaxFactory.Block(
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                            SyntaxFactory.IdentifierName("var"))
                                            .AddVariables(
                                                SyntaxFactory.VariableDeclarator(
                                                    SyntaxFactory.Identifier("moves"))
                                                    .WithInitializer(
                                                        SyntaxFactory.EqualsValueClause(
                                                            SyntaxFactory.ArrayCreationExpression(
                                                                SyntaxHelper.ArrayType(
                                                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")),
                                                                    SyntaxHelper.LiteralExpression(roles.Count))))))),
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.IdentifierName("moves")))));
                }

                return declarations.ToArray();
            }

            private MemberDeclarationSyntax CreateTrueRelationDeclaration(RelationInfo @true) =>
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxFactory.Identifier(this.result.GameStateScope.TryGetPublic(@true)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("value"))
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
                                            SyntaxFactory.IdentifierName("state")),
                                        SyntaxFactory.IdentifierName("Contains")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName("value"))))));

            private MemberDeclarationSyntax CreateDistinctRelationDeclaration(RelationInfo distinct) =>
                SyntaxFactory.MethodDeclaration(
                    this.Reference(distinct.ReturnType),
                    this.result.GameStateScope.TryGetPublic(distinct))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("a"))
                            .WithType(SyntaxHelper.ObjectType),
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("b"))
                            .WithType(SyntaxHelper.ObjectType))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.PrefixUnaryExpression(
                                        SyntaxKind.LogicalNotExpression,
                                        SyntaxHelper.ObjectEqualsExpression(SyntaxFactory.IdentifierName("a"), SyntaxFactory.IdentifierName("b")))))));

            private MethodDeclarationSyntax CreateGetAvailableMovesDeclaration(RelationInfo legal, RelationInfo role, LogicalInfo terminal)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;
                var scope = new Scope<VariableInfo>();
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
                                        SyntaxFactory.IdentifierName("moves"),
                                        SyntaxFactory.IdentifierName("Add")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.ElementAccessExpression(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ThisExpression(),
                                                                SyntaxFactory.IdentifierName("Players")))
                                                        .AddArgumentListArguments(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.CastExpression(
                                                                    SyntaxFactory.PredefinedType(
                                                                        SyntaxFactory.Token(SyntaxKind.IntKeyword)),
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
                                                    SyntaxFactory.IdentifierName("moves")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.CastExpression(
                                                            SyntaxFactory.PredefinedType(
                                                                SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                                                            this.ConvertExpression(implicated.Arguments[0], s1)))),
                                            SyntaxFactory.ConstantPattern(
                                                SyntaxHelper.Null)),
                                        SyntaxFactory.Block(
                                            addStatement))
                                    : addStatement,
                            };
                        },
                        scope));

                return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("IReadOnlyList"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))))),
                    SyntaxFactory.Identifier("GetAvailableMoves"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier("moves"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.GenericName(
                                                            SyntaxFactory.Identifier("List"))
                                                            .AddTypeArgumentListArguments(
                                                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))))
                                                        .WithArgumentList(
                                                            SyntaxFactory.ArgumentList()))))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.PrefixUnaryExpression(
                                    SyntaxKind.LogicalNotExpression,
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(this.result.GameStateScope.TryGetPublic(terminal))))),
                                statements),
                            SyntaxFactory.ReturnStatement(
                            SyntaxFactory.IdentifierName("moves"))));
            }

            private MethodDeclarationSyntax CreateGetWinnersDeclaration(RelationInfo goal, LogicalInfo terminal) =>
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("IReadOnlyCollection"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName("PlayerToken")))),
                    SyntaxFactory.Identifier("GetWinners"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.PrefixUnaryExpression(
                                    SyntaxKind.LogicalNotExpression,
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(this.result.GameStateScope.TryGetPublic(terminal))))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.GenericName(
                                                    SyntaxFactory.Identifier("List"))
                                                    .WithTypeArgumentList(
                                                        SyntaxFactory.TypeArgumentList(
                                                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                                SyntaxFactory.IdentifierName("PlayerToken")))))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList()))))),
                            SyntaxFactory.ThrowStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.IdentifierName("NotImplementedException"))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList()))
                                .WithThrowKeyword(
                                    SyntaxFactory.Token(
                                        SyntaxFactory.TriviaList(
                                            SyntaxFactory.Comment("// TODO: Enumerate the GOAL(role, value) relation and find the pairs that maximize value.")),
                                        SyntaxKind.ThrowKeyword,
                                        SyntaxFactory.TriviaList()))));

            private MethodDeclarationSyntax CreateMakeMoveDeclaration(RelationInfo next, ExpressionType stateType, RelationInfo role, RelationInfo does, ObjectInfo noop)
            {
                var roles = ((EnumType)role.Arguments[0].ReturnType).Objects;
                var scope = new Scope<object>()
                    .Add("move", ScopeFlags.Private, "move");

                var makeMove = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("IGameState"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))))),
                    SyntaxFactory.Identifier("MakeMove"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier(scope.TryGetPrivate("move")))
                                    .WithType(
                                        SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move"))))));

                if (roles.Count > 1)
                {
                    scope = scope
                        .Add("role", ScopeFlags.Private, "role")
                        .Add("moves", ScopeFlags.Private, "moves");

                    makeMove = makeMove
                        .AddBodyStatements(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier(scope.TryGetPrivate("role")))
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName("Players")),
                                                        SyntaxFactory.IdentifierName("IndexOf")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName(scope.TryGetPrivate("move")),
                                                                SyntaxFactory.IdentifierName("PlayerToken")))))))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.LogicalOrExpression,
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.NotEqualsExpression,
                                        SyntaxFactory.ElementAccessExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(scope.TryGetPrivate("moves"))))
                                            .WithArgumentList(
                                                SyntaxFactory.BracketedArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.IdentifierName(scope.TryGetPrivate("role")))))),
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
                                                        SyntaxFactory.IdentifierName("GetAvailableMoves"))),
                                                SyntaxFactory.IdentifierName("Any")))
                                            .AddArgumentListArguments(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.SimpleLambdaExpression(
                                                        SyntaxFactory.Parameter(
                                                            SyntaxFactory.Identifier("m")),
                                                        SyntaxHelper.ObjectEqualsExpression(
                                                            SyntaxFactory.IdentifierName("m"),
                                                            SyntaxFactory.IdentifierName(scope.TryGetPrivate("move")))))))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ThrowStatement(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName("ArgumentOutOfRangeException"))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.InvocationExpression(
                                                            SyntaxFactory.IdentifierName("nameof"))
                                                            .AddArgumentListArguments(
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.IdentifierName(scope.TryGetPrivate("move")))))))))),
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.IdentifierName("var"))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier(scope.TryGetPrivate("moves")))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.ArrayCreationExpression(
                                                        SyntaxHelper.ArrayType(
                                                            SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("Move")),
                                                            SyntaxHelper.LiteralExpression(roles.Count))))))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("Array"),
                                        SyntaxFactory.IdentifierName("Copy")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName(scope.TryGetPrivate("moves")))),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName(scope.TryGetPrivate("moves"))),
                                        SyntaxFactory.Argument(
                                            SyntaxHelper.LiteralExpression(roles.Count)))),
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.ElementAccessExpression(
                                        SyntaxFactory.IdentifierName(scope.TryGetPrivate("moves")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName(scope.TryGetPrivate("role")))),
                                    SyntaxFactory.IdentifierName(scope.TryGetPrivate("move")))),
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(scope.TryGetPrivate("moves")),
                                        SyntaxFactory.IdentifierName("Any")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.SimpleLambdaExpression(
                                                SyntaxFactory.Parameter(
                                                    SyntaxFactory.Identifier("m")),
                                                SyntaxFactory.IsPatternExpression(
                                                    SyntaxFactory.IdentifierName("m"),
                                                    SyntaxFactory.ConstantPattern(
                                                        SyntaxHelper.Null))))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("GameState")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName("Players"))),
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName("state"))),
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.IdentifierName(scope.TryGetPrivate("moves")))))))),
                            SyntaxFactory.LocalFunctionStatement(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                                SyntaxFactory.Identifier(does.Constant.Id)) // TODO: Lookup.
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(
                                        SyntaxFactory.Identifier("r"))
                                        .WithType(
                                            this.Reference(does.Arguments[0].ReturnType)),
                                    SyntaxFactory.Parameter(
                                        SyntaxFactory.Identifier("m"))
                                        .WithType(
                                            this.Reference(does.Arguments[1].ReturnType)))
                            .WithExpressionBody(
                                SyntaxFactory.ArrowExpressionClause(
                                    SyntaxHelper.ObjectEqualsExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ElementAccessExpression(
                                                SyntaxFactory.IdentifierName(scope.TryGetPrivate("moves")))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.CastExpression(
                                                            SyntaxFactory.PredefinedType(
                                                                SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                                                            SyntaxFactory.IdentifierName("r")))),
                                            SyntaxFactory.IdentifierName("Value")),
                                        SyntaxFactory.IdentifierName("m"))))
                            .WithSemicolonToken(
                                SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
                else
                {
                    makeMove = makeMove
                        .AddBodyStatements(
                            SyntaxFactory.LocalFunctionStatement(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                                SyntaxFactory.Identifier(does.Constant.Id)) // TODO: Lookup.
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(
                                        SyntaxFactory.Identifier("r"))
                                        .WithType(
                                            this.Reference(does.Arguments[0].ReturnType)),
                                    SyntaxFactory.Parameter(
                                        SyntaxFactory.Identifier("m"))
                                        .WithType(
                                            this.Reference(does.Arguments[1].ReturnType)))
                                .WithExpressionBody(
                                    SyntaxFactory.ArrowExpressionClause(
                                        SyntaxHelper.ObjectEqualsExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(scope.TryGetPrivate("move")),
                                                SyntaxFactory.IdentifierName("Value")),
                                            SyntaxFactory.IdentifierName("m"))))
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }

                var nextIdentifier = SyntaxFactory.Identifier("next");
                var nextIdentifierName = SyntaxFactory.IdentifierName("next");

                makeMove = makeMove
                    .AddBodyStatements(
                        SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.IdentifierName("var"))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator(
                                        nextIdentifier)
                                        .WithInitializer(
                                            SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.ObjectCreationExpression(
                                                    SyntaxFactory.ParseTypeName(this.result.NamespaceScope.TryGetPublic(stateType)))
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
                                            SyntaxFactory.IdentifierName("Add")))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(
                                                this.ConvertExpression(((ImplicitRelationalSentence)i).Arguments[0], s1)))),
                            },
                            scope.SubScope<VariableInfo>()));

                if (roles.Count > 1)
                {
                    makeMove = makeMove
                        .AddBodyStatements(
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("GameState")))
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ThisExpression(),
                                                SyntaxFactory.IdentifierName("Players"))),
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
                                SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic("GameState")))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName("Players"))),
                                    SyntaxFactory.Argument(
                                        nextIdentifierName))));
                }

                return makeMove;
            }

            private MemberDeclarationSyntax CreateLogicalFunctionDeclaration(ExpressionInfo expression, ArgumentInfo[] parameters, IEnumerable<Sentence> sentences)
            {
                var nameScope = (expression as RelationInfo)?.Scope ?? new Scope<VariableInfo>();

                var methodElement = SyntaxFactory.MethodDeclaration(
                    this.Reference(expression.ReturnType),
                    this.result.GameStateScope.TryGetPublic(expression))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                foreach (var param in parameters)
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(nameScope.TryGetPrivate(param)))
                        .WithType(this.Reference(param.ReturnType)));
                }

                var returnTrue = SyntaxFactory.ReturnStatement(SyntaxHelper.True);

                foreach (var sentence in sentences)
                {
                    var implicated = sentence.GetImplicatedSentence();

                    var walker = new ScopeWalker(this.result, parameters, (this.result.AssignedTypes.VariableTypes[sentence], nameScope), this.Reference, this.ConvertExpression);
                    walker.Walk((Expression)implicated);
                    var declarations = walker.Declarations;
                    var parameterEquality = walker.ParameterEquality;
                    var scope = walker.Scope;

                    var conditions = sentence is Implication implication
                        ? implication.Antecedents
                        : ImmutableList<Sentence>.Empty;

                    var root = this.ConvertConjnuction(conditions, _ => new[] { returnTrue }, scope);

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

            private StatementSyntax[] ConvertImplicatedSentences(IEnumerable<Sentence> sentences, Func<Sentence, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>), StatementSyntax[]> getImplication, Scope<VariableInfo> scope)
            {
                var shareableConjuncts = (from sentence in sentences
                                          let s1 = (this.result.AssignedTypes.VariableTypes[sentence], scope)
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
                Func<T, Func<Sentence, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>), StatementSyntax[]>, StatementSyntax[]> Describe<T>(T template) => (ignore0, ignore1) => null;
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
                                    (null, null))); // TODO: Allow grouping by variables.
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

            private StatementSyntax[] ConvertConjnuction(ImmutableList<Sentence> conjuncts, Func<(ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>), StatementSyntax[]> inner, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope)
            {
                StatementSyntax[] GetStatement(int i, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) s1)
                {
                    if (i >= conjuncts.Count)
                    {
                        return inner(s1);
                    }

                    return new[] { this.ConvertSentence(conjuncts[i], s2 => GetStatement(i + 1, s2), s1) };
                }

                return GetStatement(0, scope);
            }

            private StatementSyntax ConvertSentence(Sentence sentence, Func<(ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>), StatementSyntax[]> inner, (ImmutableDictionary<IndividualVariable, VariableInfo> variables, Scope<VariableInfo> names) scope)
            {
                var variable = this.result.ContainedVariables[sentence].Where(v => !scope.names.ContainsKey(scope.variables[v])).FirstOrDefault();
                if (variable is object)
                {
                    var variableInfo = scope.variables[variable];
                    scope = (scope.variables, scope.names.AddPrivate(variableInfo, variable.Name));

                    return SyntaxFactory.ForEachStatement(
                        this.Reference(variableInfo.ReturnType),
                        SyntaxFactory.Identifier(scope.names.TryGetPrivate(variableInfo)),
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

            private ExpressionSyntax ConvertCondition(Sentence condition, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope)
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

            private ExpressionSyntax ConvertExpression(Term term, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope)
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

            private ExpressionSyntax ConvertVariableExpression(IndividualVariable individualVariable, (ImmutableDictionary<IndividualVariable, VariableInfo> variables, Scope<VariableInfo> names) scope) =>
                SyntaxFactory.ParseName(scope.names.TryGetPrivate(scope.variables[individualVariable]));

            private ExpressionSyntax ConvertFunctionalTermExpression(ImplicitFunctionalTerm implicitFunctionalTerm, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope) =>
                SyntaxFactory.ObjectCreationExpression( // TODO: Runtime type checks
                    SyntaxFactory.IdentifierName(this.result.NamespaceScope.TryGetPublic(this.result.AssignedTypes.GetExpressionInfo(implicitFunctionalTerm).ReturnType)))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitFunctionalTerm.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));

            private ExpressionSyntax ConvertLogicalCondition(ConstantSentence constantSentence) =>
                SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(this.result.GameStateScope.TryGetPublic(this.result.AssignedTypes.GetExpressionInfo(constantSentence)))));

            private ExpressionSyntax ConvertNegationCondition(Negation negation, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope) =>
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    this.ConvertCondition(negation.Negated, scope));

            private ExpressionSyntax ConvertDisjunctionCondition(Disjunction disjunction, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope) =>
                disjunction.Disjuncts.Select(d => this.ConvertCondition(d, scope)).Aggregate((a, b) =>
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.LogicalOrExpression,
                        a,
                        b));

            private ExpressionSyntax ConvertImplicitRelationalCondition(ImplicitRelationalSentence implicitRelationalSentence, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope)
            {
                // TODO: Runtime type checks
                if (implicitRelationalSentence.Relation.Id == "DOES" && implicitRelationalSentence.Arguments.Count == 2)
                {
                    return
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(implicitRelationalSentence.Relation.Id), // TODO: Lookup.
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>()
                                    .AddRange(implicitRelationalSentence.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));
                }
                else
                {
                    return
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxFactory.IdentifierName(this.result.GameStateScope.TryGetPublic(this.result.AssignedTypes.GetExpressionInfo(implicitRelationalSentence)))),
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>()
                                    .AddRange(implicitRelationalSentence.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));
                }
            }

            private EnumDeclarationSyntax CreateEnumTypeDeclaration(EnumType enumType)
            {
                var enumElement = SyntaxFactory.EnumDeclaration(this.result.NamespaceScope.TryGetPublic(enumType))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var obj in enumType.Objects)
                {
                    enumElement = enumElement.AddMembers(
                        SyntaxFactory.EnumMemberDeclaration(enumType.Scope.TryGetPublic(obj)));
                }

                return enumElement;
            }

            private StructDeclarationSyntax CreateFunctionTypeDeclaration(FunctionType functionType)
            {
                var functionInfo = functionType.FunctionInfo;

                var structElement = SyntaxFactory.StructDeclaration(this.result.NamespaceScope.TryGetPublic(functionInfo.ReturnType))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.IdentifierName("ITokenFormattable")),
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IComparable"))
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
                        var fieldVariable = functionInfo.Scope.TryGetPrivate(arg);
                        var fieldElement = SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                type,
                                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(fieldVariable))))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                        var parameter = SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier(fieldVariable))
                            .WithType(type);
                        constructor = constructor.AddParameterListParameters(parameter);

                        constructor = constructor.AddBodyStatements(SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxFactory.IdentifierName(fieldVariable)),
                                SyntaxFactory.IdentifierName(parameter.Identifier))));

                        var propElement = SyntaxFactory.PropertyDeclaration(type, functionInfo.Scope.TryGetPublic(arg))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration,
                                    SyntaxFactory.Block(
                                        SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ThisExpression(),
                                            SyntaxFactory.IdentifierName(fieldVariable))))));

                        structElement = structElement.AddMembers(fieldElement, propElement);
                    }

                    structElement = structElement.AddMembers(constructor);
                }

                var compareTo = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    SyntaxFactory.Identifier("CompareTo"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("other"))
                                    .WithType(
                                        this.Reference(functionType)))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier("comp"))
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
                                                SyntaxFactory.IdentifierName("comp"),
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName(functionInfo.Scope.TryGetPrivate(arg))),
                                                        SyntaxFactory.IdentifierName("CompareTo")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName("other"),
                                                                SyntaxFactory.IdentifierName(functionInfo.Scope.TryGetPrivate(arg))))))),
                                        SyntaxHelper.LiteralExpression(0)))
                                    .Aggregate((a, b) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, a, b)),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.IdentifierName("comp"))))));
                }

                compareTo = compareTo
                    .AddBodyStatements(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.IdentifierName("comp")));

                var formatTokens =
                    SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("IList"))
                        .AddTypeArgumentListArguments(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword))),
                        SyntaxFactory.Identifier("FormatTokens"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.ArrayCreationExpression(
                                    SyntaxHelper.ArrayType(
                                        SyntaxFactory.PredefinedType(
                                            SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))
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
                                                        SyntaxFactory.IdentifierName(functionInfo.Scope.TryGetPrivate(arg))),
                                                }).ToArray())
                                            .AddExpressions(
                                                SyntaxHelper.LiteralExpression(")")))))
                        .WithSemicolonToken(
                            SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                structElement = structElement.AddMembers(formatTokens, compareTo);

                return structElement;
            }

            private ClassDeclarationSyntax CreateStateTypeDeclaration(StateType stateType)
            {
                var fieldNames = stateType.Relations.Aggregate(new Scope<object>(), (s, r) => s.AddPrivate(r, r.Id));

                var classElement = SyntaxFactory.ClassDeclaration(this.result.NamespaceScope.TryGetPublic(stateType))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                var constructor = SyntaxFactory.ConstructorDeclaration(classElement.Identifier)
                    .WithBody(SyntaxFactory.Block())
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var obj in stateType.Relations)
                {
                    var type = this.Reference(obj.ReturnType);
                    var fieldVariable = fieldNames.TryGetPrivate(obj);

                    classElement = classElement.AddMembers(
                        SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("HashSet"),
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(type))))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier(fieldVariable))))
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
                                        SyntaxFactory.IdentifierName(fieldVariable)),
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("HashSet"))
                                            .AddTypeArgumentListArguments(
                                                type))
                                        .WithArgumentList(SyntaxFactory.ArgumentList()))));
                }

                var contains = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxFactory.Identifier("Contains"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("value"))
                            .WithType(
                                this.Reference(stateType)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SwitchStatement(
                                SyntaxFactory.IdentifierName("value"))
                                .WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                .WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken))
                                .AddSections(stateType.Relations.Select(obj =>
                                    SyntaxFactory.SwitchSection()
                                        .AddLabels(
                                            SyntaxFactory.CasePatternSwitchLabel(
                                                SyntaxFactory.DeclarationPattern(
                                                    this.Reference(obj.ReturnType),
                                                    SyntaxFactory.SingleVariableDesignation(
                                                        SyntaxFactory.Identifier(fieldNames.TryGetPrivate(obj)))),
                                                SyntaxFactory.Token(SyntaxKind.ColonToken)))
                                        .AddStatements(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName(fieldNames.TryGetPrivate(obj))),
                                                        SyntaxFactory.IdentifierName("Contains")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.IdentifierName(fieldNames.TryGetPrivate(obj))))))).ToArray()),
                            SyntaxFactory.ReturnStatement(
                                SyntaxHelper.False)));

                var add = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxFactory.Identifier("Add"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("value"))
                            .WithType(
                                this.Reference(stateType)))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SwitchStatement(
                                SyntaxFactory.IdentifierName("value"))
                                .WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken))
                                .WithCloseParenToken(SyntaxFactory.Token(SyntaxKind.CloseParenToken))
                                .AddSections(stateType.Relations.Select(obj =>
                                    SyntaxFactory.SwitchSection()
                                        .AddLabels(
                                            SyntaxFactory.CasePatternSwitchLabel(
                                                SyntaxFactory.DeclarationPattern(
                                                    this.Reference(obj.ReturnType),
                                                    SyntaxFactory.SingleVariableDesignation(
                                                        SyntaxFactory.Identifier(fieldNames.TryGetPrivate(obj)))),
                                                SyntaxFactory.Token(SyntaxKind.ColonToken)))
                                        .AddStatements(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.ThisExpression(),
                                                            SyntaxFactory.IdentifierName(fieldNames.TryGetPrivate(obj))),
                                                        SyntaxFactory.IdentifierName("Add")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.IdentifierName(fieldNames.TryGetPrivate(obj))))))).ToArray()),
                            SyntaxFactory.ReturnStatement(
                                SyntaxHelper.False)));

                var compareTo = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    SyntaxFactory.Identifier("CompareTo"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("other"))
                                    .WithType(
                                        SyntaxFactory.IdentifierName("State"))))) // TODO: Name resolution.
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier("comp")))),
                            SyntaxFactory.IfStatement(
                                stateType.Relations.Select(obj =>
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.NotEqualsExpression,
                                        SyntaxFactory.ParenthesizedExpression(
                                            SyntaxFactory.AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxFactory.IdentifierName("comp"),
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName("CompareUtilities"),
                                                        SyntaxFactory.IdentifierName("CompareSets")))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.ThisExpression(),
                                                                SyntaxFactory.IdentifierName(fieldNames.TryGetPrivate(obj)))),
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.IdentifierName("other"),
                                                                SyntaxFactory.IdentifierName(fieldNames.TryGetPrivate(obj))))))),
                                        SyntaxHelper.LiteralExpression(0)))
                                    .Aggregate((a, b) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, a, b)),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.IdentifierName("comp"))))),
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.IdentifierName("comp"))));

                classElement = classElement.AddMembers(constructor, add, compareTo, contains);

                return classElement;
            }

            private class ScopeWalker : SupportedExpressionsTreeWalker
            {
                private readonly CompileResult result;
                private readonly ArgumentInfo[] parameters;
                private readonly Func<ExpressionType, TypeSyntax> reference;
                private readonly Func<Term, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>), ExpressionSyntax> convertExpression;
                private ArgumentInfo param;
                private string path;

                public ScopeWalker(CompileResult result, ArgumentInfo[] parameters, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>) scope, Func<ExpressionType, TypeSyntax> reference, Func<Term, (ImmutableDictionary<IndividualVariable, VariableInfo>, Scope<VariableInfo>), ExpressionSyntax> convertExpression)
                {
                    this.result = result;
                    this.parameters = parameters;
                    this.reference = reference;
                    this.convertExpression = convertExpression;
                    this.Declarations = new List<StatementSyntax>();
                    this.ParameterEquality = new List<ExpressionSyntax>();
                    this.Scope = scope;
                }

                public List<StatementSyntax> Declarations { get; }

                public List<ExpressionSyntax> ParameterEquality { get; }

                public (ImmutableDictionary<IndividualVariable, VariableInfo> variables, Scope<VariableInfo> names) Scope { get; set; }

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
                        this.path = this.Scope.names.TryGetPrivate(this.param);
                        this.Walk((Expression)args[i]);
                    }
                }

                public override void Walk(Term term)
                {
                    if (term is IndividualVariable argVar)
                    {
                        var argVarInfo = this.Scope.variables[argVar];
                        if (this.Scope.names.ContainsKey(argVarInfo))
                        {
                            this.ParameterEquality.Add(SyntaxHelper.ObjectEqualsExpression(SyntaxFactory.ParseName(this.path), SyntaxFactory.ParseName(this.Scope.names.TryGetPrivate(argVarInfo))));
                        }
                        else
                        {
                            this.Scope = (this.Scope.variables, this.Scope.names.SetPrivate(argVarInfo, this.path));
                        }
                    }
                    else
                    {
                        if (this.result.ContainedVariables[term].Count == 0)
                        {
                            this.ParameterEquality.Add(SyntaxHelper.ObjectEqualsExpression(SyntaxFactory.ParseName(this.path), this.convertExpression(term, this.Scope)));
                        }
                        else
                        {
                            var arg = this.result.AssignedTypes.GetExpressionInfo(term);

                            // TODO: Allow arg to be a larger type than param. There's no iheritance. Will this matter? Perhaps with unions.
                            if (this.param.ReturnType != arg.ReturnType)
                            {
                                this.Scope = (this.Scope.variables, this.Scope.names.AddPrivate(out var name, $"{this.path} as {arg.ReturnType}"));

                                this.ParameterEquality.Add(
                                    SyntaxFactory.IsPatternExpression(
                                        SyntaxFactory.ParseName(this.path),
                                        SyntaxFactory.DeclarationPattern(
                                            this.reference(arg.ReturnType),
                                            SyntaxFactory.SingleVariableDesignation(
                                                SyntaxFactory.Identifier(name)))));

                                this.path = name;
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

                        this.path += "." + functionInfo.Scope.TryGetPublic(functionInfo.Arguments[i]);

                        this.Walk((Expression)args[i]);

                        this.path = originalPath;
                    }
                }
            }
        }
    }
}
