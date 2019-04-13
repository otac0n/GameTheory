// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ConvertToCodeDomPass : CompilePass
    {
        public override IList<string> BlockedByErrors => new[]
        {
            "GDL099", // TODO: Link to AssignNamesPass
        };

        public override IList<string> ErrorsProduced => new[]
        {
            "GDL100", // TODO: Create constant, etc.
        };

        public override void Run(CompileResult result)
        {
            var allTypes = new List<ExpressionType>();
            var allExpressions = new List<ExpressionInfo>();
            ExpressionTypeVisitor.Visit(result.AssignedTypes, visitExpression: allExpressions.Add, visitType: allTypes.Add);

            var renderedTypes = allTypes.Where(t =>
                t.BuiltInType == null &&
                (!(t is ObjectType) || t is FunctionType) &&
                !RequiresRuntimeCheck(t));
            var renderedExpressions = allExpressions.Where(e =>
                !(e is VariableInfo) &&
                !(e is FunctionInfo) &&
                !(e is ObjectInfo objectInfo && (objectInfo.Value is int || objectInfo.ReturnType is EnumType)));

            TypeSyntax reference(ExpressionType type)
            {
                switch (type)
                {
                    case BooleanType booleanType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));

                    case NumberType numberType:
                    case NumberRangeType numberRangeType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

                    case EnumType enumType:
                        return SyntaxFactory.ParseTypeName(enumType.Name);

                    case StateType stateType:
                        return SyntaxFactory.ParseTypeName(stateType.Name);

                    case FunctionType functionType:
                        return SyntaxFactory.ParseTypeName(functionType.Name);

                    case ObjectType objectType:
                    case UnionType unionType:
                    case IntersectionType intersectionType:
                        return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                    case ExpressionType _ when type.BuiltInType != null:
                        return SyntaxFactory.ParseTypeName(type.BuiltInType.FullName);
                }

                throw new NotSupportedException();
            }

            var root = SyntaxFactory.CompilationUnit();

            var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(result.Name))
                .WithUsings(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxFactory.IdentifierName("System"),
                                    SyntaxFactory.IdentifierName("Collections")),
                                SyntaxFactory.IdentifierName("Generic")))));

            var gameState = SyntaxFactory.ClassDeclaration("GameState")
                .WithModifiers(
                    new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            foreach (var type in renderedTypes)
            {
                switch (type)
                {
                    case EnumType enumType:
                        gameState = gameState.AddMembers(CreateEnumTypeDeclaration(enumType));
                        break;

                    case FunctionType functionType:
                        gameState = gameState.AddMembers(CreateFunctionTypeDeclaration(functionType, reference));
                        break;

                    case StateType stateType:
                        gameState = gameState.AddMembers(CreateStateTypeDeclaration(stateType, reference));
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            foreach (var expr in renderedExpressions)
            {
                switch (expr)
                {
                    case ObjectInfo objectInfo:
                        if (objectInfo.ReturnType is ObjectType && objectInfo.Value is string value)
                        {
                            gameState = gameState.AddMembers(CreateObjectDeclaration(objectInfo, value, reference));
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }

                        break;

                    case RelationInfo relationInfo:
                        gameState = gameState.AddMembers(CreateRelationFunctionDeclaration(relationInfo, reference, result.AssignedTypes.VariableTypes));
                        break;

                    case LogicalInfo logicalInfo:
                        gameState = gameState.AddMembers(CreateLogicalDeclaration(logicalInfo, reference, result.AssignedTypes.VariableTypes));
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            ns = ns.AddMembers(gameState);
            root = root.AddMembers(ns);
            result.DeclarationSyntax = root.NormalizeWhitespace();
        }

        private static FieldDeclarationSyntax CreateObjectDeclaration(ObjectInfo objectInfo, string value, Func<ExpressionType, TypeSyntax> reference)
        {
            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    reference(objectInfo.ReturnType),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(objectInfo.Id))
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value)))))))
                .WithModifiers(new SyntaxTokenList(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)));
        }

        private static MemberDeclarationSyntax CreateLogicalDeclaration(LogicalInfo logicalInfo, Func<ExpressionType, TypeSyntax> reference, ILookup<Form, (IndividualVariable, VariableInfo)> variables) =>
            CreateLogicalFunctionDeclaration(logicalInfo, Array.Empty<ArgumentInfo>(), reference, logicalInfo.Body, variables);

        private static bool RequiresRuntimeCheck(ExpressionType t)
        {
            switch (t)
            {
                case UnionType unionType:
                case IntersectionType intersectionType:
                case NumberRangeType numberRangeType:
                    return true;

                default:
                    return false;
            }
        }

        private static MemberDeclarationSyntax CreateRelationFunctionDeclaration(RelationInfo relationInfo, Func<ExpressionType, TypeSyntax> reference, ILookup<Form, (IndividualVariable, VariableInfo)> variables) =>
            CreateLogicalFunctionDeclaration(relationInfo, relationInfo.Arguments, reference, relationInfo.Body, variables);

        private static MemberDeclarationSyntax CreateLogicalFunctionDeclaration(ExpressionInfo expression, ArgumentInfo[] arguments, Func<ExpressionType, TypeSyntax> reference, IEnumerable<Sentence> sentences, ILookup<Form, (IndividualVariable, VariableInfo)> variables)
        {
            var methodElement = SyntaxFactory.MethodDeclaration(
                reference(expression.ReturnType),
                expression.Id)
                .WithModifiers(new SyntaxTokenList(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

            foreach (var arg in arguments)
            {
                methodElement = methodElement.AddParameterListParameters(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier(arg.Id.TrimStart('?'))) // TODO: Better name resolution.
                    .WithType(reference(arg.ReturnType)));
            }

            var trivia = SyntaxFactory.TriviaList();
            foreach (var sentence in sentences)
            {
                trivia = trivia.Add(SyntaxFactory.Comment($"// {sentence}"));
            }

            methodElement = methodElement.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)).WithLeadingTrivia(trivia));

            return methodElement;
        }

        private static EnumDeclarationSyntax CreateEnumTypeDeclaration(EnumType enumType)
        {
            var enumElement = SyntaxFactory.EnumDeclaration(enumType.Name)
                .WithModifiers(
                    new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            foreach (var obj in enumType.Objects)
            {
                enumElement = enumElement.AddMembers(
                    SyntaxFactory.EnumMemberDeclaration(obj.Id));
            }

            return enumElement;
        }

        private static StructDeclarationSyntax CreateFunctionTypeDeclaration(FunctionType functionType, Func<ExpressionType, TypeSyntax> reference)
        {
            var structElement = SyntaxFactory.StructDeclaration(functionType.Name)
                .WithModifiers(
                    new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            var constructor = SyntaxFactory.ConstructorDeclaration(structElement.Identifier)
                .WithModifiers(
                    new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            foreach (var arg in functionType.FunctionInfo.Arguments)
            {
                var type = reference(arg.ReturnType);
                var fieldVariable = SyntaxFactory.VariableDeclarator("_" + arg.Id.TrimStart('?')); // TODO: Better name resolution.
                var fieldElement = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        type,
                        SyntaxFactory.SingletonSeparatedList(fieldVariable)))
                    .WithModifiers(
                        new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

                var parameter = SyntaxFactory.Parameter(fieldVariable.Identifier).WithType(type);
                constructor = constructor.AddParameterListParameters(parameter);

                constructor = constructor.AddBodyStatements(SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(),
                            SyntaxFactory.IdentifierName(fieldVariable.Identifier)),
                        SyntaxFactory.IdentifierName(parameter.Identifier))));

                var propElement = SyntaxFactory.PropertyDeclaration(type, arg.Id.TrimStart('?')) // TODO: Better name resolution.
                    .WithModifiers(
                        new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration,
                            SyntaxFactory.Block(
                                SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxFactory.IdentifierName(fieldVariable.Identifier))))));

                structElement = structElement.AddMembers(fieldElement, propElement);
            }

            structElement = structElement.AddMembers(constructor);

            return structElement;
        }

        private static ClassDeclarationSyntax CreateStateTypeDeclaration(StateType stateType, Func<ExpressionType, TypeSyntax> reference)
        {
            var classElement = SyntaxFactory.ClassDeclaration(stateType.Name);

            var constructor = SyntaxFactory.ConstructorDeclaration(classElement.Identifier)
                .WithBody(SyntaxFactory.Block())
                .WithModifiers(
                    new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            foreach (var obj in stateType.Relations)
            {
                constructor = constructor.AddParameterListParameters(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier(obj.Id)).WithType(
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("IEnumerable"),
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(reference(obj.ReturnType))))));
            }

            classElement = classElement.AddMembers(constructor);

            return classElement;
        }
    }
}
