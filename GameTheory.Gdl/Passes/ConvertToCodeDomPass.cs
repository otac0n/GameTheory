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
                            gameState = gameState.AddMembers(this.CreateRelationFunctionDeclaration(relationInfo));
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

                foreach (var param in parameters)
                {
                    methodElement = methodElement.AddParameterListParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier(param.Id.TrimStart('?'))) // TODO: Better name resolution.
                        .WithType(Reference(param.ReturnType)));
                }

                var returnTrue = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

                foreach (var sentence in sentences)
                {
                    var implicated = sentence.GetImplicatedSentence();
                    var args = implicated is ImplicitRelationalSentence implicitRelationalSentence
                        ? implicitRelationalSentence.Arguments
                        : ImmutableList<Term>.Empty;
                    Debug.Assert(args.Count == parameters.Length, "Arguments' arity doesn't match parameters' arity.");

                    var parameterPairs = args.Zip(parameters, (argument, parameter) => new { argument, parameter }).ToList();

                    var renameGroups = (from p in parameterPairs
                                        let argVar = p.argument as IndividualVariable
                                        where argVar is object
                                        let param = p.parameter
                                        where param.Id != argVar.Id
                                        group param by argVar).ToList();

                    var replacements = new Dictionary<IndividualVariable, IndividualVariable>();
                    var parameterEquality = new List<(ArgumentInfo, ArgumentInfo)>();
                    foreach (var rename in renameGroups)
                    {
                        var sentenceVariable = rename.Key;
                        var fromItems = rename.ToList();
                        replacements.Add(sentenceVariable, new IndividualVariable(fromItems[0].Id));
                        parameterEquality.AddRange(fromItems.Skip(1).Select(item => (fromItems[0], item)));
                    }

                    // TODO: Add renames to avoid name collisions.

                    var sentenceVars = this.result.AssignedTypes.VariableTypes[sentence];
                    var replaced = (Sentence)new VariableReplacer(replacements).Walk((Expression)sentence); // TODO: Update the sentence variables.
                    var conditions = replaced is Implication implication
                        ? implication.Antecedents
                        : ImmutableList<Sentence>.Empty;

                    StatementSyntax root = returnTrue;

                    root = conditions.Aggregate(root, (inner, condition) => this.ConvertSentence(condition, inner));

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
                                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                SyntaxFactory.Argument(SyntaxFactory.ParseName(pair.Item1.Id.TrimStart('?'))), // TODO: Better name resolution.
                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                SyntaxFactory.Argument(SyntaxFactory.ParseName(pair.Item2.Id.TrimStart('?'))), // TODO: Better name resolution.
                                            })))),
                            SyntaxFactory.Block(root))
                            .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {implicated}")));
                    }

                    methodElement = methodElement.AddBodyStatements(root);
                }

                methodElement = methodElement.AddBodyStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

                return methodElement;
            }

            private StatementSyntax ConvertSentence(Sentence sentence, StatementSyntax inner) =>
                SyntaxFactory.IfStatement(
                    this.ConvertCondition(sentence),
                    SyntaxFactory.Block(inner))
                .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Comment($"// {sentence}")));

            private ExpressionSyntax ConvertCondition(Sentence condition)
            {
                switch (condition)
                {
                    case ImplicitRelationalSentence implicitRelationalSentence:
                        return this.ConvertImplicitRelationalCondition(implicitRelationalSentence);

                    case ConstantSentence constantSentence:
                        return this.ConvertLogicalCondition(constantSentence);

                    case Negation negation:
                        return this.ConvertNegationCondition(negation);

                    default:
                        throw new NotSupportedException();
                }
            }

            private ExpressionSyntax ConvertExpression(Term term)
            {
                switch (term)
                {
                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.ConvertFunctionalTermExpression(implicitFunctionalTerm);

                    case IndividualVariable individualVariable:
                        return this.ConvertVariableExpression(individualVariable);

                    case Constant constant:
                        return this.ConvertConstantExpression(constant);

                    default:
                        throw new NotSupportedException();
                }
            }

            private ExpressionSyntax ConvertConstantExpression(Constant constant) =>
                CreateObjectReference((ObjectInfo)this.result.AssignedTypes.ExpressionTypes[(constant.Id, 0)]);

            private ExpressionSyntax ConvertVariableExpression(IndividualVariable individualVariable) =>
                SyntaxFactory.ParseName(individualVariable.Id.TrimStart('?')); // TODO: Better name resolution.

            private ExpressionSyntax ConvertFunctionalTermExpression(ImplicitFunctionalTerm implicitFunctionalTerm) =>
                SyntaxFactory.ObjectCreationExpression( // TODO: Runtime type checks
                    SyntaxFactory.IdentifierName(implicitFunctionalTerm.Function.Id))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitFunctionalTerm.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg))))));

            private ExpressionSyntax ConvertLogicalCondition(ConstantSentence constantSentence) =>
                SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(constantSentence.Constant.Id)));

            private ExpressionSyntax ConvertNegationCondition(Negation negation) =>
                SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    this.ConvertCondition(negation.Negated));

            private ExpressionSyntax ConvertImplicitRelationalCondition(ImplicitRelationalSentence implicitRelationalSentence) =>
                SyntaxFactory.InvocationExpression( // TODO: Runtime type checks
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(implicitRelationalSentence.Relation.Id)),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>()
                            .AddRange(implicitRelationalSentence.Arguments.Select(arg => SyntaxFactory.Argument(this.ConvertExpression(arg))))));

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
