namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Generic;

    internal static class ExpressionTypeVisitor
    {
        public static void Visit(IEnumerable<ExpressionInfo> rootExpressions, Action<ExpressionInfo> visitExpression = null, Action<ExpressionType> visitType = null)
        {
            var queue = new Queue<ExpressionInfo>();
            var seen = new HashSet<ExpressionInfo>();
            var seenTypes = new HashSet<ExpressionType>();

            void Add(ExpressionInfo expr)
            {
                if (seen.Add(expr))
                {
                    queue.Enqueue(expr);
                }
            }

            void AddAll(IEnumerable<ExpressionInfo> expressions)
            {
                foreach (var expr in expressions)
                {
                    Add(expr);
                }
            }

            void AddType(ExpressionType type)
            {
                if (type != null && seenTypes.Add(type))
                {
                    visitType?.Invoke(type);

                    switch (type)
                    {
                        case UnionType unionType:
                            AddAll(unionType.Expressions);
                            break;

                        case IntersectionType intersectionType:
                            AddAll(intersectionType.Expressions);
                            break;

                        case StructType structType:
                            AddAll(structType.Objects);
                            break;

                        case EnumType enumType:
                            AddAll(enumType.Objects);
                            break;

                        case ObjectType objectType:
                        case NoneType noneType:
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    AddType(type.BaseType);
                }
            }

            AddAll(rootExpressions);
            while (queue.Count > 0)
            {
                var expression = queue.Dequeue();
                visitExpression?.Invoke(expression);
                AddType(expression.ReturnType);
                switch (expression)
                {
                    case ExpressionWithArgumentsInfo expressionWithArgumentsInfo:
                        AddAll(expressionWithArgumentsInfo.Arguments);
                        break;

                    case VariableInfo variableInfo:
                    case ObjectInfo objectInfo:
                    case LogicalInfo logicalInfo:
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
