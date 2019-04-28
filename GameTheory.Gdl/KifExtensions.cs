// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using KnowledgeInterchangeFormat.Expressions;

    internal static class KifExtensions
    {
        public static (Constant constant, int arity) GetImplicatedConstantWithArity(this Sentence form)
        {
            switch (form)
            {
                case ConstantSentence constantSentence:
                    return (constantSentence.Constant, 0);
                case ImplicitRelationalSentence implicitRelationalSentence:
                    return (implicitRelationalSentence.Relation, implicitRelationalSentence.Arguments.Count);
                case Implication implication:
                    return GetImplicatedConstantWithArity(implication.Consequent);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Sentence GetImplicatedSentence(this Sentence form)
        {
            switch (form)
            {
                case ConstantSentence constantSentence:
                    return constantSentence;
                case ImplicitRelationalSentence implicitRelationalSentence:
                    return implicitRelationalSentence;
                case Implication implication:
                    return GetImplicatedSentence(implication.Consequent);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
