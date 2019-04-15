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

            public void Run()
            {
                var allTypes = new List<ExpressionType>();
                var allExpressions = new List<ExpressionInfo>();
                ExpressionTypeVisitor.Visit(this.result.AssignedTypes, visitExpression: allExpressions.Add, visitType: allTypes.Add);

                var renderedTypes = allTypes.Where(t =>
                    t.BuiltInType == null &&
                    (!(t is ObjectType) || t is FunctionType) &&
                    !RequiresRuntimeCheck(t));
                var renderedExpressions = allExpressions.Where(e =>
                    !(e is VariableInfo) &&
                    !(e is FunctionInfo) &&
                    !(e is ObjectInfo objectInfo && (objectInfo.Value is int || objectInfo.ReturnType is EnumType)));

                var root = SyntaxFactory.CompilationUnit();

                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(this.result.Name))
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
                            gameState = gameState.AddMembers(CreateFunctionTypeDeclaration(functionType, Reference));
                            break;

                        case StateType stateType:
                            gameState = gameState.AddMembers(CreateStateTypeDeclaration(stateType, Reference));
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
                                gameState = gameState.AddMembers(CreateObjectDeclaration(objectInfo, value));
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }

                            break;

                        case RelationInfo relationInfo:
                            switch (relationInfo.Id)
                            {
                                case "INIT":
                                case "NEXT":
                                    break;

                                default:
                                    gameState = gameState.AddMembers(this.CreateRelationFunctionDeclaration(relationInfo));
                                    break;
                            }

                            break;

                        case LogicalInfo logicalInfo:
                            gameState = gameState.AddMembers(this.CreateLogicalDeclaration(logicalInfo));
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                ns = ns.AddMembers(gameState);
                root = root.AddMembers(ns);
                this.result.DeclarationSyntax = root.NormalizeWhitespace();
            }

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
                    .WithModifiers(new SyntaxTokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)));

            private static ExpressionSyntax CreateObjectReference(ObjectInfo objectInfo) =>
                SyntaxFactory.ParseName(objectInfo.Id); // TODO: Enums, etc.

            private MemberDeclarationSyntax CreateLogicalDeclaration(LogicalInfo logicalInfo) =>
                this.CreateLogicalFunctionDeclaration(logicalInfo, Array.Empty<ArgumentInfo>(), logicalInfo.Body);

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

            private MemberDeclarationSyntax CreateRelationFunctionDeclaration(RelationInfo relationInfo) =>
                this.CreateLogicalFunctionDeclaration(relationInfo, relationInfo.Arguments, relationInfo.Body);

            private MemberDeclarationSyntax CreateLogicalFunctionDeclaration(ExpressionInfo expression, ArgumentInfo[] parameters, IEnumerable<Sentence> sentences)
            {
                var methodElement = SyntaxFactory.MethodDeclaration(
                    Reference(expression.ReturnType),
                    expression.Id)
                    .WithModifiers(new SyntaxTokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

                var parameterNames = parameters.ToDictionary(p => p, p => p.Id.TrimStart('?')); // TODO: Better name resolution.

                foreach (var param in parameters)
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterNames[param]))
                        .WithType(Reference(param.ReturnType)));
                }

                var returnTrue = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

                foreach (var sentence in sentences)
                {
                    var scope = ImmutableDictionary<IndividualVariable, NameSyntax>.Empty;

                    var implicated = sentence.GetImplicatedSentence();
                    var args = implicated is ImplicitRelationalSentence implicitRelationalSentence
                        ? implicitRelationalSentence.Arguments
                        : ImmutableList<Term>.Empty;
                    Debug.Assert(args.Count == parameters.Length, "Arguments' arity doesn't match parameters' arity.");

                    var parameterEquality = new List<(NameSyntax, ExpressionSyntax)>();
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        void X(Term expr, ArgumentInfo param, NameSyntax path)
                        {
                            if (expr is IndividualVariable argVar)
                            {
                                if (scope.TryGetValue(argVar, out var matching))
                                {
                                    parameterEquality.Add((path, matching));
                                }
                                else
                                {
                                    scope = scope.SetItem(argVar, path);
                                }
                            }
                            else
                            {
                                var arg = this.result.AssignedTypes.GetExpressionInfo(expr);

                                // TODO: Allow arg to be a larger type than param. There's no iheritance. Will this matter?
                                if (param.ReturnType != arg.ReturnType)
                                {
                                    // TODO: Type check for arg.ReturnType and change `path`
                                }

                                X();

                                // TODO: Should be recursive to support pulling variables from arguments.
                                parameterEquality.Add((path, this.ConvertExpression(expr, scope)));
                            }
                        }

                        var p = parameters[i];
                        X(args[i], p, SyntaxFactory.ParseName(parameterNames[p]));
                    }

                    var conditions = sentence is Implication implication
                        ? implication.Antecedents
                        : ImmutableList<Sentence>.Empty;

                    StatementSyntax root = returnTrue;

                    root = conditions.Aggregate(root, (inner, condition) => this.ConvertSentence(condition, inner, scope));

                    if (parameterEquality.Count > 0)
                    {
                        root = SyntaxFactory.IfStatement(
                            parameterEquality.Aggregate((ExpressionSyntax)null, (expr, pair) =>
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
                                                SyntaxFactory.Argument(pair.Item1),
                                                SyntaxFactory.Argument(pair.Item2),
                                            })))),
                            SyntaxFactory.Block(root))
                            .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {implicated}")));
                    }

                    methodElement = methodElement.AddBodyStatements(root);
                }

                methodElement = methodElement.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

                return methodElement;
            }

            private StatementSyntax ConvertSentence(Sentence sentence, StatementSyntax inner, ImmutableDictionary<IndividualVariable, NameSyntax> scope) =>
                SyntaxFactory.IfStatement(
                    this.ConvertCondition(sentence, scope),
                    SyntaxFactory.Block(inner))
                .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}")));

            private ExpressionSyntax ConvertCondition(Sentence condition, ImmutableDictionary<IndividualVariable, NameSyntax> scope)
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

            private ExpressionSyntax ConvertExpression(Term term, ImmutableDictionary<IndividualVariable, NameSyntax> scope)
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

            private ExpressionSyntax ConvertVariableExpression(IndividualVariable individualVariable, ImmutableDictionary<IndividualVariable, NameSyntax> scope) =>
                scope[individualVariable];

            private ExpressionSyntax ConvertFunctionalTermExpression(ImplicitFunctionalTerm implicitFunctionalTerm, ImmutableDictionary<IndividualVariable, NameSyntax> scope) =>
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

            private ExpressionSyntax ConvertNegationCondition(Negation negation, ImmutableDictionary<IndividualVariable, NameSyntax> scope) =>
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    this.ConvertCondition(negation.Negated, scope));

            private ExpressionSyntax ConvertImplicitRelationalCondition(ImplicitRelationalSentence implicitRelationalSentence, ImmutableDictionary<IndividualVariable, NameSyntax> scope) =>
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
}
