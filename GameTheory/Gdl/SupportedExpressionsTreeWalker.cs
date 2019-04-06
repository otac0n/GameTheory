// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    internal abstract class SupportedExpressionsTreeWalker : ExpressionTreeWalker
    {
        public sealed override void Walk(CharacterBlock characterBlock) => throw new NotSupportedException();

        public sealed override void Walk(CharacterReference characterReference) => throw new NotSupportedException();

        public sealed override void Walk(CharacterString characterString) => throw new NotSupportedException();

        public sealed override void Walk(CompleteDefinition completeDefinition) => throw new NotSupportedException();

        public sealed override void Walk(CompleteFunctionDefinition completeFunctionDefinition) => throw new NotSupportedException();

        public sealed override void Walk(CompleteLogicalDefinition completeLogicalDefinition) => throw new NotSupportedException();

        public sealed override void Walk(CompleteObjectDefinition completeObjectDefinition) => throw new NotSupportedException();

        public sealed override void Walk(CompleteRelationDefinition completeRelationDefinition) => throw new NotSupportedException();

        public sealed override void Walk(ConditionalTerm conditionalTerm) => throw new NotSupportedException();

        public sealed override void Walk(Definition definition) => throw new NotSupportedException();

        public sealed override void Walk(Equation equation) => throw new NotSupportedException();

        public sealed override void Walk(Equivalence equivalence) => throw new NotSupportedException();

        public sealed override void Walk(ExistentiallyQuantifiedSentence existentiallyQuantifiedSentence) => throw new NotSupportedException();

        public sealed override void Walk(ExplicitFunctionalTerm explicitFunctionalTerm) => throw new NotSupportedException();

        public sealed override void Walk(ExplicitRelationalSentence explicitRelationalSentence) => throw new NotSupportedException();

        public sealed override void Walk(IfTerm ifTerm) => throw new NotSupportedException();

        public sealed override void Walk(Inequality inequality) => throw new NotSupportedException();

        public sealed override void Walk(ListExpression listExpression) => throw new NotSupportedException();

        public sealed override void Walk(ListTerm listTerm) => throw new NotSupportedException();

        public sealed override void Walk(LogicalPair logicalPair) => throw new NotSupportedException();

        public sealed override void Walk(LogicalTerm logicalTerm) => throw new NotSupportedException();

        public sealed override void Walk(PartialDefinition partialDefinition) => throw new NotSupportedException();

        public sealed override void Walk(PartialFunctionDefinition partialFunctionDefinition) => throw new NotSupportedException();

        public sealed override void Walk(PartialLogicalDefinition partialLogicalDefinition) => throw new NotSupportedException();

        public sealed override void Walk(PartialObjectDefinition partialObjectDefinition) => throw new NotSupportedException();

        public sealed override void Walk(PartialRelationDefinition partialRelationDefinition) => throw new NotSupportedException();

        public sealed override void Walk(QuantifiedSentence quantifiedSentence) => throw new NotSupportedException();

        public sealed override void Walk(Quotation quotation) => throw new NotSupportedException();

        public sealed override void Walk(ReversePartialFunctionDefinition reversePartialFunctionDefinition) => throw new NotSupportedException();

        public sealed override void Walk(ReversePartialLogicalDefinition reversePartialLogicalDefinition) => throw new NotSupportedException();

        public sealed override void Walk(ReversePartialObjectDefinition reversePartialObjectDefinition) => throw new NotSupportedException();

        public sealed override void Walk(ReversePartialRelationDefinition reversePartialRelationDefinition) => throw new NotSupportedException();

        public sealed override void Walk(SequenceVariable sequenceVariable) => throw new NotSupportedException();

        public sealed override void Walk(UniversallyQuantifiedSentence universallyQuantifiedSentence) => throw new NotSupportedException();

        public sealed override void Walk(UnrestrictedDefinition unrestrictedDefinition) => throw new NotSupportedException();

        public sealed override void Walk(UnrestrictedFunctionDefinition unrestrictedFunctionDefinition) => throw new NotSupportedException();

        public sealed override void Walk(UnrestrictedLogicalDefinition unrestrictedLogicalDefinition) => throw new NotSupportedException();

        public sealed override void Walk(UnrestrictedObjectDefinition unrestrictedObjectDefinition) => throw new NotSupportedException();

        public sealed override void Walk(UnrestrictedRelationDefinition unrestrictedRelationDefinition) => throw new NotSupportedException();

        public sealed override void Walk(VariableSpecification variableSpecification) => throw new NotSupportedException();

        public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
        {
            foreach (var term in implicitRelationalSentence.Arguments)
            {
                this.Walk(term);
            }
        }

        public override void Walk(ImplicitFunctionalTerm implicitFunctionalTerm)
        {
            foreach (var term in implicitFunctionalTerm.Arguments)
            {
                this.Walk(term);
            }
        }
    }
}
