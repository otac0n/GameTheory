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
        /// <inheritdoc/>
        public override IList<string> BlockedByErrors => new[]
        {
            "GDL099", // TODO: Link to AssignNamesPass
        };

        /// <inheritdoc/>
        public override IList<string> ErrorsProduced => new[]
        {
            "GDL100", // TODO: Create constant, etc.
        };

        /// <inheritdoc/>
        public override void Run(CompileResult result)
        {
            new Runner(result).Run();
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

            public static TypeSyntax Reference(ExpressionType type)
            {
                switch (type)
                {
                    case BooleanType booleanType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));

                    case NumberType numberType:
                    case NumberRangeType numberRangeType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

                    case EnumType enumType:
                        return SyntaxFactory.ParseTypeName(enumType.Name); // TODO: Name resolution.

                    case FunctionType functionType:
                        return SyntaxFactory.ParseTypeName(functionType.Name); // TODO: Name resolution.

                    case StateType stateType:
                    case ObjectType objectType:
                    case UnionType unionType:
                    case IntersectionType intersectionType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                    case ExpressionType _ when type.BuiltInType != null:
                        return SyntaxFactory.ParseTypeName(type.BuiltInType.FullName);
                }

                throw new NotSupportedException();
            }

            public static ExpressionSyntax AllMembers(ExpressionType type, ExpressionType declaredAs)
            {
                switch (type)
                {
                    case NumberType numberType:
                        return EnumerableRangeExpression(0, 101);

                    case NumberRangeType numberRangeType:
                        return EnumerableRangeExpression(numberRangeType.Start, numberRangeType.End - numberRangeType.Start + 1);

                    case EnumType enumType:
                        return SyntaxFactory.ParenthesizedExpression(
                            SyntaxFactory.CastExpression(
                                ArrayType(Reference(declaredAs)),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("Enum"),
                                        SyntaxFactory.IdentifierName("GetValues")),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.TypeOfExpression(
                                                    SyntaxFactory.ParseTypeName(enumType.Name))))))));

                    case UnionType unionType:
                        return unionType.Expressions
                            .Select(expr =>
                            {
                                switch (expr)
                                {
                                    case ObjectInfo objectInfo:
                                        return SyntaxFactory.ArrayCreationExpression(
                                            ArrayType(Reference(declaredAs)),
                                            SyntaxFactory.InitializerExpression(
                                                SyntaxKind.ArrayInitializerExpression,
                                                SyntaxFactory.SingletonSeparatedList(
                                                    CreateObjectReference(objectInfo))));

                                    default:
                                        return AllMembers(expr.ReturnType, declaredAs);
                                }
                            })
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

                throw new NotSupportedException();
            }

            public void Run()
            {
                var allTypes = new List<ExpressionType>();
                var allExpressions = new List<ExpressionInfo>();
                ExpressionTypeVisitor.Visit(this.result.AssignedTypes, visitExpression: allExpressions.Add, visitType: allTypes.Add);

                var init = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("INIT", 1)];
                var @true = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("TRUE", 1)];
                var next = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("NEXT", 1)];
                var role = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("ROLE", 1)];
                var does = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("DOES", 2)];
                var legal = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("LEGAL", 2)];
                var goal = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("GOAL", 2)];
                var distinct = (RelationInfo)this.result.AssignedTypes.ExpressionTypes[("DISTINCT", 2)];

                var renderedTypes = allTypes.Where(t =>
                    t.BuiltInType == null &&
                    (!(t is ObjectType) || t is FunctionType) &&
                    (!RequiresRuntimeCheck(t) || t is StateType));
                var renderedExpressions = allExpressions.Except(init, @true, does, next, distinct).Where(e =>
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

                var move = CreateMoveTypeDeclaration(does.Arguments[1].ReturnType);

                var gameState = SyntaxFactory.ClassDeclaration("GameState")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IGameState"))
                            .AddTypeArgumentListArguments(
                                SyntaxFactory.IdentifierName("Move"))))
                    .AddMembers(
                        this.CreateGameStateConstructorDeclaration(init, (StateType)init.Arguments[0].ReturnType, role),
                        this.CreateMakeMoveDeclaration(next),
                        this.CreateGetWinnersDeclaration(goal),
                        this.CreateGetAvailableMovesDeclaration(legal),
                        this.CreateStateDeclaration((StateType)init.Arguments[0].ReturnType),
                        this.CreateTrueRelationDeclaration(@true))
                    .AddMembers(CreateSharedGameStateDeclarations(distinct))
                    .AddMembers(
                        renderedExpressions.Select(expr =>
                        {
                            switch (expr)
                            {
                                case ObjectInfo objectInfo:
                                    if (objectInfo.ReturnType is ObjectType && objectInfo.Value is string value)
                                    {
                                        return CreateObjectDeclaration(objectInfo, value);
                                    }

                                    break;

                                case RelationInfo relationInfo:
                                    return this.CreateLogicalFunctionDeclaration(relationInfo, relationInfo.Arguments, relationInfo.Body);

                                case LogicalInfo logicalInfo:
                                    return this.CreateLogicalFunctionDeclaration(logicalInfo, Array.Empty<ArgumentInfo>(), logicalInfo.Body);
                            }

                            throw new InvalidOperationException();
                        }).ToArray());

                ns = ns
                    .AddMembers(gameState, move)
                    .AddMembers(
                        renderedTypes.Select(type =>
                        {
                            switch (type)
                            {
                                case EnumType enumType:
                                    return CreateEnumTypeDeclaration(enumType);

                                case FunctionType functionType:
                                    return CreateFunctionTypeDeclaration(functionType);

                                case StateType stateType:
                                    return CreateStateTypeDeclaration(stateType);
                            }

                            return (MemberDeclarationSyntax)null ?? throw new InvalidOperationException();
                        }).ToArray());
                root = root.AddMembers(ns);
                this.result.DeclarationSyntax = root.NormalizeWhitespace();
            }

            private static MemberDeclarationSyntax[] CreateSharedGameStateDeclarations(RelationInfo distinct) => new MemberDeclarationSyntax[]
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
                                            SyntaxFactory.IdentifierName("Move")))),
                    SyntaxFactory.Identifier("GetOutcomes"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("move"))
                                .WithType(
                                    SyntaxFactory.IdentifierName("Move")))))
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
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.IdentifierName("move")))))),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(1)))))))),
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("IEnumerable"))
                        .AddTypeArgumentListArguments(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IGameState"))
                                .AddTypeArgumentListArguments(
                                    SyntaxFactory.IdentifierName("Move"))),
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
                            SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("other"))
                                    .WithType(
                                        SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("IGameState"))
                                            .AddTypeArgumentListArguments(
                                                SyntaxFactory.IdentifierName("Move"))))))
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
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(0)))))),
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
                                                SyntaxFactory.IdentifierName("GameState")))))),
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
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NullLiteralExpression))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(1)))))),
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
                SyntaxFactory.MethodDeclaration(
                    Reference(distinct.ReturnType),
                    distinct.Id)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("a"))
                            .WithType(
                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))),
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("b"))
                            .WithType(
                                SyntaxFactory.PredefinedType(
                                    SyntaxFactory.Token(SyntaxKind.ObjectKeyword))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ReturnStatement(
                                    SyntaxFactory.PrefixUnaryExpression(
                                        SyntaxKind.LogicalNotExpression,
                                        ObjectEqualsExpression(SyntaxFactory.IdentifierName("a"), SyntaxFactory.IdentifierName("b"))))))),
            };

            private static ClassDeclarationSyntax CreateMoveTypeDeclaration(ExpressionType moveType) =>
                SyntaxFactory.ClassDeclaration("Move")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxFactory.IdentifierName("IMove")))))
                    .AddMembers(
                        SyntaxFactory.ConstructorDeclaration(
                            SyntaxFactory.Identifier("Move"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("playerToken"))
                                    .WithType(SyntaxFactory.IdentifierName("PlayerToken")),
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                                    .WithType(Reference(moveType)))
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
                                                        SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)))))))),
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
                                                    SyntaxFactory.ArrayType(
                                                        SyntaxFactory.PredefinedType(
                                                            SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                                                        .AddRankSpecifiers(
                                                            SyntaxFactory.ArrayRankSpecifier(
                                                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                                    SyntaxFactory.OmittedArraySizeExpression()))))
                                                    .WithInitializer(
                                                        SyntaxFactory.InitializerExpression(
                                                            SyntaxKind.ArrayInitializerExpression,
                                                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.ThisExpression(),
                                                                    SyntaxFactory.IdentifierName("Value")))))))))),
                        SyntaxFactory.PropertyDeclaration(
                            Reference(moveType),
                            SyntaxFactory.Identifier("Value"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAccessorListAccessors(
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))));

            private static ObjectCreationExpressionSyntax NewPlayerTokenExpression() =>
                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("PlayerToken"))
                    .WithArgumentList(SyntaxFactory.ArgumentList());

            private static ExpressionSyntax ObjectEqualsExpression(ExpressionSyntax left, ExpressionSyntax right) =>
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.PredefinedType(
                            SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                        SyntaxFactory.IdentifierName("Equals")),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(new ArgumentSyntax[]
                            {
                                SyntaxFactory.Argument(left),
                                SyntaxFactory.Argument(right),
                            })));

            private static ExpressionSyntax EnumerableRangeExpression(int start, int count) =>
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Enumerable"),
                        SyntaxFactory.IdentifierName("Range")))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>().AddRange(new ArgumentSyntax[]
                            {
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(start))),
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(count))),
                            })));

            private static ArrayTypeSyntax ArrayType(TypeSyntax elementType) =>
                SyntaxFactory.ArrayType(
                    elementType,
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.ArrayRankSpecifier(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.OmittedArraySizeExpression()))));

            private static FieldDeclarationSyntax CreateObjectDeclaration(ObjectInfo objectInfo, string value) =>
                SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        Reference(objectInfo.ReturnType),
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(objectInfo.Id))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value)))))))
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            private static ExpressionSyntax CreateObjectReference(ObjectInfo objectInfo)
            {
                switch (objectInfo.ReturnType)
                {
                    case EnumType enumType:
                        return SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(enumType.Name),
                            SyntaxFactory.IdentifierName(objectInfo.Id));

                    case NumberType numberType when objectInfo.Value is int:
                        return SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((int)objectInfo.Value));

                    default:
                        return SyntaxFactory.IdentifierName(objectInfo.Id);
                }
            }

            private ConstructorDeclarationSyntax CreateGameStateConstructorDeclaration(RelationInfo init, StateType stateType, RelationInfo role)
            {
                var constructor = SyntaxFactory.ConstructorDeclaration(
                    SyntaxFactory.Identifier("GameState"))
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
                                            Enumerable.Range(0, ((EnumType)role.Arguments[0].ReturnType).Objects.Count)
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
                                        SyntaxFactory.ParseTypeName(stateType.Name))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList())))));

                foreach (var sentence in init.Body)
                {
                    var implicated = (ImplicitRelationalSentence)sentence.GetImplicatedSentence();
                    var scope = ImmutableDictionary<IndividualVariable, ExpressionSyntax>.Empty;

                    Func<ImmutableDictionary<IndividualVariable, ExpressionSyntax>, StatementSyntax> addState = s =>
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
                                        this.ConvertExpression(implicated.Arguments[0], s))));

                    var sentenceVariables = this.result.AssignedTypes.VariableTypes[sentence].ToDictionary(v => v.Item1, v => v.Item2);
                    switch (sentence)
                    {
                        case Implication implication:
                            constructor = constructor
                                .AddBodyStatements(
                                    this.ConvertConjnuction(
                                        implication.Antecedents,
                                        addState,
                                        sentenceVariables,
                                        scope));
                            break;

                        default:
                            constructor = constructor
                                .AddBodyStatements(
                                    addState(scope));
                            break;
                    }
                }

                return constructor;
            }

            private MemberDeclarationSyntax CreateStateDeclaration(StateType stateType) =>
                SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName(stateType.Name))
                        .AddVariables(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier("state"))))
                    .AddModifiers(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            private MemberDeclarationSyntax CreateTrueRelationDeclaration(RelationInfo @true) =>
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxFactory.Identifier(@true.Id))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("value"))
                            .WithType(
                                Reference(@true.Arguments[0].ReturnType)))
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

            private MethodDeclarationSyntax CreateGetAvailableMovesDeclaration(RelationInfo legal) =>
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("IReadOnlyList"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName("Move")))),
                    SyntaxFactory.Identifier("GetAvailableMoves"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("TERMINAL"))),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.ArrayCreationExpression(
                                                SyntaxFactory.ArrayType(
                                                    SyntaxFactory.IdentifierName("Move"))
                                                    .AddRankSpecifiers(
                                                        SyntaxFactory.ArrayRankSpecifier(
                                                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.NumericLiteralExpression,
                                                                    SyntaxFactory.Literal(0)))))))))),
                            SyntaxFactory.ThrowStatement(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.IdentifierName("NotImplementedException"))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList()))
                                .WithThrowKeyword(
                                    SyntaxFactory.Token(
                                        SyntaxFactory.TriviaList(
                                            SyntaxFactory.Comment("// TODO: Enumerate the LEGAL(role, move) relation")),
                                        SyntaxKind.ThrowKeyword,
                                        SyntaxFactory.TriviaList()))));

            private MethodDeclarationSyntax CreateGetWinnersDeclaration(RelationInfo goal) =>
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
                                            SyntaxFactory.IdentifierName("TERMINAL")))),
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

            private MethodDeclarationSyntax CreateMakeMoveDeclaration(RelationInfo next) =>
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("IGameState"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName("Move")))),
                    SyntaxFactory.Identifier("MakeMove"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("move"))
                                    .WithType(
                                        SyntaxFactory.IdentifierName("Move")))))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ThrowStatement(
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.IdentifierName("NotImplementedException"))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList()))
                                    .WithThrowKeyword(
                                        SyntaxFactory.Token(
                                            SyntaxFactory.TriviaList(
                                                SyntaxFactory.Comment("// TODO: Enumerate the NEXT(state) relation")),
                                            SyntaxKind.ThrowKeyword,
                                            SyntaxFactory.TriviaList())))));

            private MemberDeclarationSyntax CreateLogicalFunctionDeclaration(ExpressionInfo expression, ArgumentInfo[] parameters, IEnumerable<Sentence> sentences)
            {
                var nameScope = (expression as RelationInfo)?.Scope ?? new Scope<ArgumentInfo>();

                var methodElement = SyntaxFactory.MethodDeclaration(
                    Reference(expression.ReturnType),
                    expression.Id) // TODO: Name resolution.
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

                foreach (var param in parameters)
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(nameScope.TryGetPrivate(param)))
                        .WithType(Reference(param.ReturnType)));
                }

                var returnTrue = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

                foreach (var sentence in sentences)
                {
                    var implicated = sentence.GetImplicatedSentence();

                    var scope = ImmutableDictionary<IndividualVariable, ExpressionSyntax>.Empty;
                    var walker = new ScopeWalker(this.result, parameters, nameScope, scope, this.ConvertExpression);
                    walker.Walk((Expression)implicated);
                    var declarations = walker.Declarations;
                    var parameterEquality = walker.ParameterEquality;
                    scope = walker.Scope;

                    var sentenceVariables = this.result.AssignedTypes.VariableTypes[sentence].ToDictionary(v => v.Item1, v => v.Item2);

                    var conditions = sentence is Implication implication
                        ? implication.Antecedents
                        : ImmutableList<Sentence>.Empty;

                    var root = this.ConvertConjnuction(conditions, _ => returnTrue, sentenceVariables, scope);

                    if (parameterEquality.Count > 0)
                    {
                        root = SyntaxFactory.IfStatement(
                            parameterEquality.Aggregate((left, right) =>
                                SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right)),
                            SyntaxFactory.Block(root));
                    }

                    root = SyntaxFactory.Block(declarations)
                        .AddStatements(root)
                        .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}")));

                    methodElement = methodElement.AddBodyStatements(root);
                }

                // TODO: Runtime type checks
                methodElement = methodElement.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

                return methodElement;
            }

            private StatementSyntax ConvertConjnuction(ImmutableList<Sentence> conditions, Func<ImmutableDictionary<IndividualVariable, ExpressionSyntax>, StatementSyntax> inner, Dictionary<IndividualVariable, VariableInfo> sentenceVariables, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope)
            {
                StatementSyntax GetStatement(int i, ImmutableDictionary<IndividualVariable, ExpressionSyntax> s1)
                {
                    if (i >= conditions.Count)
                    {
                        return inner(s1);
                    }

                    return this.ConvertSentence(conditions[i], s2 => GetStatement(i + 1, s2), sentenceVariables, s1);
                }

                return GetStatement(0, scope);
            }

            private StatementSyntax ConvertSentence(Sentence sentence, Func<ImmutableDictionary<IndividualVariable, ExpressionSyntax>, StatementSyntax> inner, Dictionary<IndividualVariable, VariableInfo> sentenceVariables, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope)
            {
                var variables = this.result.ContainedVariables[sentence].Except(scope.Keys);
                if (variables.Count > 0)
                {
                    var variable = (IndividualVariable)variables.First();
                    var variableInfo = sentenceVariables[variable];
                    var name = variable.Name.TrimStart('?'); // TODO: Avoid conflicts.
                    scope = scope.SetItem(variable, SyntaxFactory.IdentifierName(name));

                    return SyntaxFactory.ForEachStatement(
                        Reference(variableInfo.ReturnType),
                        SyntaxFactory.Identifier(name),
                        AllMembers(variableInfo.ReturnType, variableInfo.ReturnType),
                        SyntaxFactory.Block(
                            this.ConvertSentence(sentence, inner, sentenceVariables, scope)));
                }
                else
                {
                    return SyntaxFactory.IfStatement(
                        this.ConvertCondition(sentence, scope),
                        SyntaxFactory.Block(inner(scope)))
                    .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}")));
                }
            }

            private ExpressionSyntax ConvertCondition(Sentence condition, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope)
            {
                switch (condition)
                {
                    case ImplicitRelationalSentence implicitRelationalSentence:
                        return this.ConvertImplicitRelationalCondition(implicitRelationalSentence, scope);

                    case ConstantSentence constantSentence:
                        return this.ConvertLogicalCondition(constantSentence);

                    case Negation negation:
                        return this.ConvertNegationCondition(negation, scope);

                    default:
                        throw new NotSupportedException();
                }
            }

            private ExpressionSyntax ConvertExpression(Term term, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope)
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
                        throw new NotSupportedException();
                }
            }

            private ExpressionSyntax ConvertConstantExpression(Constant constant) =>
                CreateObjectReference((ObjectInfo)this.result.AssignedTypes.ExpressionTypes[(constant.Id, 0)]);

            private ExpressionSyntax ConvertVariableExpression(IndividualVariable individualVariable, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                scope[individualVariable];

            private ExpressionSyntax ConvertFunctionalTermExpression(ImplicitFunctionalTerm implicitFunctionalTerm, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                SyntaxFactory.ObjectCreationExpression( // TODO: Runtime type checks
                    SyntaxFactory.IdentifierName(implicitFunctionalTerm.Function.Id))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitFunctionalTerm.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));

            private ExpressionSyntax ConvertLogicalCondition(ConstantSentence constantSentence) =>
                SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(constantSentence.Constant.Id)));

            private ExpressionSyntax ConvertNegationCondition(Negation negation, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    this.ConvertCondition(negation.Negated, scope));

            private ExpressionSyntax ConvertImplicitRelationalCondition(ImplicitRelationalSentence implicitRelationalSentence, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope) =>
                SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(implicitRelationalSentence.Relation.Id)),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitRelationalSentence.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg, scope))))));

            private static EnumDeclarationSyntax CreateEnumTypeDeclaration(EnumType enumType)
            {
                var enumElement = SyntaxFactory.EnumDeclaration(enumType.Name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var obj in enumType.Objects)
                {
                    enumElement = enumElement.AddMembers(
                        SyntaxFactory.EnumMemberDeclaration(obj.Id));
                }

                return enumElement;
            }

            private static StructDeclarationSyntax CreateFunctionTypeDeclaration(FunctionType functionType)
            {
                var functionInfo = functionType.FunctionInfo;

                var structElement = SyntaxFactory.StructDeclaration(functionType.Name) // TODO: Name resolution.
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddBaseListTypes(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IComparable"))
                            .AddTypeArgumentListArguments(
                                Reference(functionType))));

                var constructor = SyntaxFactory.ConstructorDeclaration(structElement.Identifier)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var arg in functionInfo.Arguments)
                {
                    var type = Reference(arg.ReturnType);
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

                var compareTo = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    SyntaxFactory.Identifier("CompareTo"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                                SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("other"))
                                    .WithType(
                                        Reference(functionType)))))
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
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(0))))
                                    .Aggregate((a, b) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, a, b)),
                                SyntaxFactory.Block(
                                    SyntaxFactory.SingletonList<StatementSyntax>(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.IdentifierName("comp"))))),
                            SyntaxFactory.ReturnStatement(
                                SyntaxFactory.IdentifierName("comp"))));

                structElement = structElement.AddMembers(constructor, compareTo);

                return structElement;
            }

            private static ClassDeclarationSyntax CreateStateTypeDeclaration(StateType stateType)
            {
                var fieldNames = stateType.Relations.Aggregate(new Scope<ExpressionInfo>(), (s, r) => s.AddPrivate(r, r.Id));

                var classElement = SyntaxFactory.ClassDeclaration(stateType.Name) // TODO: Name resolution.
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                var constructor = SyntaxFactory.ConstructorDeclaration(classElement.Identifier)
                    .WithBody(SyntaxFactory.Block())
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var obj in stateType.Relations)
                {
                    var type = Reference(obj.ReturnType);
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
                                Reference(stateType)))
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
                                                    Reference(obj.ReturnType),
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
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.FalseLiteralExpression))));

                var add = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    SyntaxFactory.Identifier("Add"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("value"))
                            .WithType(
                                Reference(stateType)))
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
                                                    Reference(obj.ReturnType),
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
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.FalseLiteralExpression))));

                var compareTo = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    SyntaxFactory.Identifier("CompareTo"))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(
                        SyntaxFactory.ParameterList(
                            SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
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
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(0))))
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
                private readonly Scope<ArgumentInfo> parameterNames;
                private readonly Func<Term, ImmutableDictionary<IndividualVariable, ExpressionSyntax>, ExpressionSyntax> convertExpression;
                private ArgumentInfo param;
                private ExpressionSyntax path;

                public ScopeWalker(CompileResult result, ArgumentInfo[] parameters, Scope<ArgumentInfo> parameterNames, ImmutableDictionary<IndividualVariable, ExpressionSyntax> scope, Func<Term, ImmutableDictionary<IndividualVariable, ExpressionSyntax>, ExpressionSyntax> convertExpression)
                {
                    this.result = result;
                    this.parameters = parameters;
                    this.parameterNames = parameterNames;
                    this.convertExpression = convertExpression;
                    this.Declarations = new List<StatementSyntax>();
                    this.ParameterEquality = new List<ExpressionSyntax>();
                    this.Scope = scope;
                }

                public List<StatementSyntax> Declarations { get; }

                public List<ExpressionSyntax> ParameterEquality { get; }

                public ImmutableDictionary<IndividualVariable, ExpressionSyntax> Scope { get; set; }

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
                        this.path = SyntaxFactory.ParseName(this.parameterNames.TryGetPrivate(this.param));
                        this.Walk((Expression)args[i]);
                    }
                }

                public override void Walk(Term term)
                {
                    if (term is IndividualVariable argVar)
                    {
                        if (this.Scope.TryGetValue(argVar, out var matching))
                        {
                            this.ParameterEquality.Add(ObjectEqualsExpression(this.path, matching));
                        }
                        else
                        {
                            this.Scope = this.Scope.SetItem(argVar, this.path);
                        }
                    }
                    else
                    {
                        if (this.result.ContainedVariables[term].Count == 0)
                        {
                            this.ParameterEquality.Add(ObjectEqualsExpression(this.path, this.convertExpression(term, this.Scope)));
                        }
                        else
                        {
                            var arg = this.result.AssignedTypes.GetExpressionInfo(term);

                            // TODO: Allow arg to be a larger type than param. There's no iheritance. Will this matter?
                            if (this.param.ReturnType != arg.ReturnType)
                            {
                                var name = $"as{arg.ReturnType}"; // TODO: Better name resolution.
                                var typedVar = SyntaxFactory.IdentifierName(name);
                                this.Declarations.Add(
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(
                                            Reference(arg.ReturnType))
                                        .AddVariables(
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier(name)))));
                                this.ParameterEquality.Add(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.LogicalAndExpression,
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.IsExpression,
                                            this.path,
                                            Reference(arg.ReturnType)),
                                        SyntaxFactory.BinaryExpression(
                                            SyntaxKind.IsExpression,
                                            SyntaxFactory.ParenthesizedExpression(
                                                SyntaxFactory.AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    typedVar,
                                                    SyntaxFactory.CastExpression(
                                                        Reference(arg.ReturnType),
                                                        this.path))),
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))));
                                this.path = typedVar;
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

                        this.path = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            this.path,
                            SyntaxFactory.IdentifierName(functionInfo.Scope.TryGetPublic(functionInfo.Arguments[i])));

                        this.Walk((Expression)args[i]);

                        this.path = originalPath;
                    }
                }
            }
        }
    }
}
