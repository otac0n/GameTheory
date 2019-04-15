// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;

    public class AssignedTypes : IEnumerable<ExpressionInfo>
    {
        public AssignedTypes(Dictionary<(string, int), ExpressionInfo> expressionTypes, ILookup<Form, (IndividualVariable, VariableInfo)> variableTypes)
        {
            this.ExpressionTypes = expressionTypes;

            this.VariableTypes = variableTypes;
        }

        public Dictionary<(string, int), ExpressionInfo> ExpressionTypes { get; }

        public ILookup<Form, (IndividualVariable, VariableInfo)> VariableTypes { get; }

        public ExpressionInfo GetExpressionInfo(Expression expression)
        {
            switch (expression)
            {
                case Constant constant:
                    return this.ExpressionTypes[(constant.Id, 0)];

                case ConstantSentence constantSentence:
                    return this.ExpressionTypes[(constantSentence.Constant.Id, 0)];

                case ImplicitRelationalSentence implicitRelationalSentence:
                    return this.ExpressionTypes[(implicitRelationalSentence.Relation.Id, implicitRelationalSentence.Arguments.Count)];

                case ImplicitFunctionalTerm implicitFunctionalTerm:
                    return this.ExpressionTypes[(implicitFunctionalTerm.Function.Id, implicitFunctionalTerm.Arguments.Count)];

                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerator<ExpressionInfo> GetEnumerator() => this.ExpressionTypes.Values.Concat(this.VariableTypes.SelectMany(x => x).Select(x => x.Item2)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
