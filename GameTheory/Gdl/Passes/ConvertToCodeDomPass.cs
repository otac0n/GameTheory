// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;

    internal class ConvertToCodeDomPass : CompilePass
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

        public override IList<string> ErrorsProduced => Array.Empty<string>();

        public override void Run(CompileResult result)
        {
            var bodies = from f in result.KnowledgeBase.Forms
                         group f by GetImplicatedConstantWithArity((Sentence)f);

            var allTypes = new List<ExpressionType>();
            var allExpressions = new List<ExpressionInfo>();
            ExpressionTypeVisitor.Visit(result.ExpressionTypes.Values, visitExpression: allExpressions.Add, visitType: allTypes.Add);

            var renderedTypes = allTypes.Where(t =>
                t.BuiltInType == null &&
                (!(t is ObjectType) || t is FunctionType) &&
                !(t is UnionType) &&
                !(t is IntersectionType) &&
                !(t is NumberRangeType));
            var renderedExpressions = allExpressions.Where(e =>
                !(e is VariableInfo) &&
                !(e is FunctionInfo) &&
                !(e is ObjectInfo objectInfo && (objectInfo.Value is int || objectInfo.ReturnType is EnumType)));

            CodeTypeReference reference(ExpressionType type)
            {
                if (type.BuiltInType != null)
                {
                    return new CodeTypeReference(type.BuiltInType);
                }

                switch (type)
                {
                    case BooleanType booleanType:
                        return new CodeTypeReference(typeof(bool));

                    case NumberType numberType:
                    case NumberRangeType numberRangeType:
                        return new CodeTypeReference(typeof(int));

                    case EnumType enumType:
                        return new CodeTypeReference(enumType.Name);

                    case StateType stateType:
                        return new CodeTypeReference(stateType.Name);

                    case FunctionType functionType:
                        return new CodeTypeReference(functionType.Name);

                    case ObjectType objectType:
                    case UnionType unionType:
                    case IntersectionType intersectionType:
                        return new CodeTypeReference(typeof(object));
                }

                throw new NotSupportedException();
            }

            var root = new CodeCompileUnit();
            var ns = new CodeNamespace
            {
                Name = result.Name,
            };
            root.Namespaces.Add(ns);

            var gameState = new CodeTypeDeclaration
            {
                Name = "GameState",
            };
            ns.Types.Add(gameState);

            foreach (var type in renderedTypes)
            {
                switch (type)
                {
                    case EnumType enumType:
                        gameState.Members.Add(CreateEnumTypeDeclaration(enumType));
                        break;

                    case FunctionType functionType:
                        gameState.Members.Add(CreateFunctionTypeDeclaration(functionType, reference));
                        break;

                    case StateType stateType:
                        var classElement = CreateStateTypeDeclaration(stateType, reference);

                        gameState.Members.Add(classElement);
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
                        if (objectInfo.ReturnType is ObjectType && objectInfo.Value is string)
                        {
                            gameState.Members.Add(new CodeMemberField
                            {
                                Name = objectInfo.Id,
                                Type = reference(objectInfo.ReturnType),
                                InitExpression = new CodePrimitiveExpression(objectInfo.Value),
                                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                            });
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }

                        break;

                    case RelationInfo relationInfo:
                        gameState.Members.Add(CreateRelationFunctionDeclaration(relationInfo, reference));
                        break;

                    case LogicalInfo logicalInfo:
                        gameState.Members.Add(new CodeMemberMethod
                        {
                            Name = logicalInfo.Id,
                            ReturnType = reference(logicalInfo.ReturnType),
                            Statements =
                            {
                                new CodeMethodReturnStatement(new CodePrimitiveExpression(false)),
                            },
                        });
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            result.CodeCompileUnit = root;
        }

        private static CodeMemberMethod CreateRelationFunctionDeclaration(RelationInfo relationInfo, Func<ExpressionType, CodeTypeReference> reference)
        {
            var methodElement = new CodeMemberMethod
            {
                Name = relationInfo.Id,
                ReturnType = reference(relationInfo.ReturnType),
            };

            foreach (var arg in relationInfo.Arguments)
            {
                methodElement.Parameters.Add(new CodeParameterDeclarationExpression
                {
                    Name = arg.Id,
                    Type = reference(arg.ReturnType),
                });
            }

            methodElement.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

            return methodElement;
        }

        private static CodeTypeDeclaration CreateEnumTypeDeclaration(EnumType enumType)
        {
            var enumElement = new CodeTypeDeclaration
            {
                Name = enumType.Name,
                IsEnum = true,
            };

            foreach (var obj in enumType.Objects)
            {
                enumElement.Members.Add(new CodeMemberField
                {
                    Name = obj.Id,
                });
            }

            return enumElement;
        }

        private static CodeTypeDeclaration CreateFunctionTypeDeclaration(FunctionType functionType, Func<ExpressionType, CodeTypeReference> reference)
        {
            var structElement = new CodeTypeDeclaration
            {
                Name = functionType.Name,
                IsStruct = true,
            };

            var constructor = new CodeConstructor();
            structElement.Members.Add(constructor);

            foreach (var arg in functionType.FunctionInfo.Arguments)
            {
                var type = reference(arg.ReturnType);
                var fieldElement = new CodeMemberField
                {
                    Name = "_" + arg.Id,
                    Type = type,
                    Attributes = MemberAttributes.Private,
                };

                constructor.Parameters.Add(new CodeParameterDeclarationExpression
                {
                    Name = fieldElement.Name,
                    Type = type,
                });
                constructor.Statements.Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldElement.Name),
                    new CodeArgumentReferenceExpression(fieldElement.Name)));

                var propElement = new CodeMemberProperty
                {
                    Name = arg.Id,
                    Type = type,
                    GetStatements =
                    {
                        new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldElement.Name)),
                    },
                };

                structElement.Members.Add(fieldElement);
                structElement.Members.Add(propElement);
            }

            return structElement;
        }

        private static CodeTypeDeclaration CreateStateTypeDeclaration(StateType stateType, Func<ExpressionType, CodeTypeReference> reference)
        {
            var classElement = new CodeTypeDeclaration
            {
                Name = stateType.Name,
            };

            var constructor = new CodeConstructor();

            foreach (var obj in stateType.Relations)
            {
                constructor.Parameters.Add(new CodeParameterDeclarationExpression
                {
                    Name = obj.Id,
                    Type = new CodeTypeReference(typeof(IEnumerable<>))
                    {
                        TypeArguments =
                        {
                            reference(obj.ReturnType),
                        },
                    },
                });
            }

            return classElement;
        }

        private static (string id, int arity) GetImplicatedConstantWithArity(Sentence form)
        {
            switch (form)
            {
                case ConstantSentence constantSentence:
                    return (constantSentence.Constant.Id, 0);
                case ImplicitRelationalSentence implicitRelationalSentence:
                    return (implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count);
                case Implication implication:
                    return GetImplicatedConstantWithArity(implication.Consequent);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
