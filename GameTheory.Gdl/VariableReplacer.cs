// <copyright file="VariableReplacer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;

    /// <summary>
    /// Replaces variables in an <see cref="Expression"/> tree.
    /// </summary>
    internal class VariableReplacer : ExpressionTreeReplacer
    {
        private readonly Dictionary<IndividualVariable, IndividualVariable> replacements;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableReplacer"/> class.
        /// </summary>
        /// <param name="replacements">The replacements to perform.</param>
        public VariableReplacer(Dictionary<IndividualVariable, IndividualVariable> replacements)
        {
            this.replacements = replacements;
        }

        /// <inheritdoc/>
        public override Expression Walk(IndividualVariable individualVariable, ImmutableStack<string> path) =>
            this.replacements.TryGetValue(individualVariable, out var replacement)
                ? replacement
                : individualVariable;
    }
}
