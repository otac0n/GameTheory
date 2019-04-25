// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;

    public class AssignedTypes : IEnumerable<ExpressionInfo>
    {
        public AssignedTypes(ImmutableDictionary<(Constant, int), ConstantInfo> expressionTypes, ImmutableDictionary<Sentence, ImmutableDictionary<IndividualVariable, VariableInfo>> variableTypes)
        {
            this.ExpressionTypes = expressionTypes;

            this.VariableTypes = variableTypes;
        }

        public ImmutableDictionary<(Constant, int), ConstantInfo> ExpressionTypes { get; }

        public ImmutableDictionary<Sentence, ImmutableDictionary<IndividualVariable, VariableInfo>> VariableTypes { get; }

        public ConstantInfo GetExpressionInfo(Expression expression)
        {
            switch (expression)
            {
                case Constant constant:
                    return this.ExpressionTypes[(constant, 0)];

                case ConstantSentence constantSentence:
                    return this.ExpressionTypes[(constantSentence.Constant, 0)];

                case ImplicitRelationalSentence implicitRelationalSentence:
                    return this.ExpressionTypes[(implicitRelationalSentence.Relation, implicitRelationalSentence.Arguments.Count)];

                case ImplicitFunctionalTerm implicitFunctionalTerm:
                    return this.ExpressionTypes[(implicitFunctionalTerm.Function, implicitFunctionalTerm.Arguments.Count)];

                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerator<ExpressionInfo> GetEnumerator() => this.ExpressionTypes.Values.Cast<ExpressionInfo>().Concat(this.VariableTypes.Values.SelectMany(v => v.Values)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
