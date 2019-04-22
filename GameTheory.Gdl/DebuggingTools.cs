namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;

    internal static class DebuggingTools
    {
        public static string RenderTypeGraph(AssignedTypes assignedTypes)
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
                ? expressionWithArgumentsInfo.ToString()
                : expression is ArgumentInfo argumentInfo
                    ? $"{exprId(argumentInfo.Expression)}:{varId(argumentInfo)}"
                    : expression is VariableInfo variableInfo
                        ? varId(variableInfo)
                        : expression.ToString();

            ExpressionTypeVisitor.Visit(
                assignedTypes.ExpressionTypes.Values.Where(v => !(v is ObjectInfo objectInfo && objectInfo.Value is int)),
                expression =>
                {
                    switch (expression)
                    {
                        case ExpressionWithArgumentsInfo expressionWithArgumentsInfo:
                            sb.Append($"{exprId(expression)} [shape={(expression is RelationInfo ? "tab" : "box")} label=<<table border=\"0\"><tr><td colspan=\"{expressionWithArgumentsInfo.Arity}\" >{expressionWithArgumentsInfo}</td></tr><tr>");

                            foreach (var arg in expressionWithArgumentsInfo.Arguments)
                            {
                                sb.Append($"<td port=\"{varId(arg)}\">{arg}</td>");
                            }

                            sb.AppendLine("</tr></table>>]");
                            break;

                        case ArgumentInfo argumentInfo:
                            break;

                        case VariableInfo variableInfo:
                            sb.AppendLine($"{exprId(variableInfo)} [shape=parallelogram label=\"{variableInfo}\"];");
                            break;

                        case ObjectInfo objectInfo:
                            sb.AppendLine($"{exprId(objectInfo)} [shape=box label=\"{objectInfo}\"];");
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

                        case StateType structType:
                            foreach (var expr in structType.Relations)
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

        public static string RenderNameGraph(KnowledgeBase knowledgeBase, AssignedTypes assignedTypes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph {");

            var ids = new Dictionary<ExpressionType, string>();
            var varIds = new Dictionary<VariableInfo, string>();
            string varId(VariableInfo variable) => varIds.TryGetValue(variable, out var value) ? value : varIds[variable] = "v" + varIds.Count.ToString();
            string exprId(ExpressionInfo expression) => expression is ExpressionWithArgumentsInfo expressionWithArgumentsInfo
                ? expressionWithArgumentsInfo.ToString()
                : expression is ArgumentInfo argumentInfo
                    ? $"{exprId(argumentInfo.Expression)}:{varId(argumentInfo)}"
                    : expression is VariableInfo variableInfo
                        ? varId(variableInfo)
                        : expression.ToString();

            ExpressionTypeVisitor.Visit(
                assignedTypes.Where(v => !(v is ObjectInfo objectInfo && objectInfo.Value is int)),
                expression =>
                {
                    switch (expression)
                    {
                        case ExpressionWithArgumentsInfo expressionWithArgumentsInfo:
                            sb.Append($"{exprId(expression)} [shape={(expression is RelationInfo ? "tab" : "box")} label=<<table border=\"0\"><tr><td colspan=\"{expressionWithArgumentsInfo.Arity}\" >{expressionWithArgumentsInfo}</td></tr><tr>");

                            foreach (var arg in expressionWithArgumentsInfo.Arguments)
                            {
                                sb.Append($"<td port=\"{varId(arg)}\">{arg}</td>");
                            }

                            sb.AppendLine("</tr></table>>]");

                            break;

                        case ArgumentInfo argumentInfo:
                            break;

                        case VariableInfo variableInfo:
                            sb.AppendLine($"{exprId(variableInfo)} [shape=parallelogram label=\"{variableInfo}\"];");
                            break;
                    }
                },
                type =>
                {
                });

            new VariableNameWalker(sb, assignedTypes, exprId, varId).Walk((Expression)knowledgeBase);

            sb.AppendLine("}");
            return sb.ToString();
        }

        private class VariableNameWalker : SupportedExpressionsTreeWalker
        {
            private readonly ImmutableDictionary<(string, int), ExpressionInfo> expressionTypes;
            private readonly Func<ExpressionInfo, string> exprId;
            private readonly ImmutableDictionary<Sentence, ImmutableDictionary<IndividualVariable, VariableInfo>> containedVariables;
            private readonly Func<VariableInfo, string> varId;
            private readonly StringBuilder sb;
            private ImmutableDictionary<IndividualVariable, VariableInfo> variableTypes;
            private VariableDirection variableDirection;

            public VariableNameWalker(StringBuilder sb, AssignedTypes assignedTypes, Func<ExpressionInfo, string> exprId, Func<VariableInfo, string> varId)
            {
                this.sb = sb;
                this.expressionTypes = assignedTypes.ExpressionTypes;
                this.exprId = exprId;
                this.containedVariables = assignedTypes.VariableTypes;
                this.varId = varId;
            }

            private enum VariableDirection
            {
                None = 0,
                In,
                Out,
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                var relationInfo = (RelationInfo)this.expressionTypes[(implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count)];
                this.AddNameUsages(implicitRelationalSentence.Arguments, relationInfo);
                base.Walk(implicitRelationalSentence);
            }

            public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
            {
                var functionInfo = (FunctionInfo)this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];
                this.AddNameUsages(implicitFunctionalTerm.Arguments, functionInfo);
                base.Walk(implicitFunctionalTerm);
            }

            public override void Walk(Implication implication)
            {
                var previousDirection = this.variableDirection;
                try
                {
                    this.variableDirection = VariableDirection.In;
                    if (implication.Antecedents != null)
                    {
                        foreach (var sentence in implication.Antecedents)
                        {
                            this.Walk((Expression)sentence);
                        }
                    }

                    this.variableDirection = VariableDirection.Out;
                    if (implication.Consequent != null)
                    {
                        this.Walk((Expression)implication.Consequent);
                    }
                }
                finally
                {
                    this.variableDirection = previousDirection;
                }
            }

            public override void Walk(KnowledgeBase knowledgeBase)
            {
                this.variableDirection = VariableDirection.Out;

                foreach (var form in knowledgeBase.Forms)
                {
                    this.variableTypes = this.containedVariables[(Sentence)form];
                    this.Walk((Expression)form);
                    this.variableTypes = null;
                }

                this.variableDirection = VariableDirection.None;
            }

            private void AddNameUsages(ImmutableList<Term> arguments, ExpressionWithArgumentsInfo relationInfo)
            {
                var arity = arguments.Count;
                for (var i = 0; i < arity; i++)
                {
                    var source = relationInfo.Arguments[i];
                    var target = this.GetExpressionInfo(arguments[i]);

                    if (!(target is VariableInfo))
                    {
                        continue;
                    }

                    this.sb.AppendLine($"{this.exprId(source)} -> {this.exprId(target)};");
                }
            }

            private ExpressionInfo GetExpressionInfo(Term arg)
            {
                switch (arg)
                {
                    case Constant constant:
                        return this.expressionTypes[(constant.Id, 0)];

                    case ImplicitFunctionalTerm implicitFunctionalTerm:
                        return this.expressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];

                    case IndividualVariable individualVariable:
                        return this.variableTypes[individualVariable];

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
