namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using System.Text;
    using GameTheory.Gdl.Types;

    internal static class DebuggingTools
    {
        public static string RenderTypeGraph(IEnumerable<ExpressionInfo> rootExpressions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph {");
            sb.AppendLine("subgraph cluster_1 { NEXT_1; INIT_1; TRUE_1 }");
            sb.AppendLine("subgraph cluster_2 { ROLE_1; DOES_2; LEGAL_2; GOAL_2 }");

            var ids = new Dictionary<ExpressionType, string>();
            var varIds = new Dictionary<VariableInfo, string>();
            string typeId(ExpressionType type) => ids.TryGetValue(type, out var value) ? value : ids[type] = "t" + ids.Count.ToString();
            string varId(VariableInfo variable) => varIds.TryGetValue(variable, out var value) ? value : varIds[variable] = "v" + varIds.Count.ToString();
            string exprId(ExpressionInfo expression) => expression is ExpressionWithArgumentsInfo expressionWithArgumentsInfo
                ? $"{expressionWithArgumentsInfo.Id}_{expressionWithArgumentsInfo.Arity}"
                : expression is ArgumentInfo argumentInfo
                    ? $"{exprId(argumentInfo.Expression)}:{argumentInfo.Id}"
                    : expression is VariableInfo variableInfo
                        ? varId(variableInfo)
                        : expression.Id;

            ExpressionTypeVisitor.Visit(
                rootExpressions,
                expression =>
                {
                    switch (expression)
                    {
                        case ExpressionWithArgumentsInfo expressionWithArgumentsInfo:
                            sb.Append($"{exprId(expression)} [shape={(expression is RelationInfo ? "tab" : "box")} label=<<table border=\"0\"><tr><td colspan=\"{expressionWithArgumentsInfo.Arity}\" >{expressionWithArgumentsInfo.Id}</td></tr><tr>");

                            foreach (var arg in expressionWithArgumentsInfo.Arguments)
                            {
                                sb.Append($"<td port=\"{arg.Id}\">{arg.Id}</td>");
                            }

                            sb.AppendLine("</tr></table>>]");
                            break;

                        case ArgumentInfo argumentInfo:
                            break;

                        case VariableInfo variableInfo:
                            sb.AppendLine($"{exprId(variableInfo)} [shape=parallelogram label=\"{variableInfo.Id}\"];");
                            break;

                        case ObjectInfo objectInfo:
                            sb.AppendLine($"{exprId(objectInfo)} [shape=box label=\"{objectInfo.Id}\"];");
                            break;
                    }

                    if (expression.ReturnType != null)
                    {
                        sb.AppendLine($"{exprId(expression)} -> {typeId(expression.ReturnType)}{(expression is RelationInfo ? " [style=dotted weight=0]" : "")};");
                    }
                    else
                    {
                        sb.AppendLine($"{exprId(expression)} -> {exprId(expression)}_null;");
                        sb.AppendLine($"{exprId(expression)}_null [label=null];");
                    }
                },
                type =>
                {
                    sb.AppendLine($"{typeId(type)} [label=\"{type}\"];");
                    switch (type)
                    {
                        case UnionType unionType:
                            foreach (var expr in unionType.Expressions)
                            {
                                sb.AppendLine($"{typeId(type)} -> {exprId(expr)};");
                            }

                            break;

                        case IntersectionType intersectionType:
                            foreach (var expr in intersectionType.Expressions)
                            {
                                sb.AppendLine($"{typeId(type)} -> {exprId(expr)};");
                            }

                            break;

                        case StructType structType:
                            foreach (var expr in structType.Objects)
                            {
                                sb.AppendLine($"{typeId(type)} -> {exprId(expr)};");
                            }

                            break;

                        case EnumType enumType:
                            foreach (var expr in enumType.Objects)
                            {
                                sb.AppendLine($"{typeId(type)} -> {exprId(expr)} [style=dotted weight=0];");
                            }

                            break;
                    }

                    sb.AppendLine($"{typeId(type)} -> {typeId(type.BaseType)} [style=dotted weight=0];");
                });

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
