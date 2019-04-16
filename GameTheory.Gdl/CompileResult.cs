// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using KnowledgeInterchangeFormat.Expressions;
    using Microsoft.CodeAnalysis.CSharp;
    using Pegasus.Common;

    public class CompileResult
    {
        private readonly Lazy<Dictionary<(string, int), ConstantType>> constantTypes;
        private readonly Lazy<Dictionary<Expression, ImmutableHashSet<Variable>>> containedVariables;
        private readonly Lazy<AssignedTypes> assignedTypes;

        public CompileResult(string name, KnowledgeBase knowledgeBase)
        {
            this.Name = name;
            this.KnowledgeBase = knowledgeBase;
            this.containedVariables = new Lazy<Dictionary<Expression, ImmutableHashSet<Variable>>>(() => ContainedVariablesAnalyzer.Analyze(this.KnowledgeBase));
            this.constantTypes = new Lazy<Dictionary<(string, int), ConstantType>>(() => ConstantArityAnalyzer.Analyze(this.KnowledgeBase));
            this.assignedTypes = new Lazy<AssignedTypes>(() => AssignTypesAnalyzer.Analyze(this.KnowledgeBase, this.ConstantTypes, this.ContainedVariables));

            this.AtomicSentences = new Dictionary<Sentence, bool>();
            this.DatalogTerms = new Dictionary<Term, bool>();
            this.DatalogLiterals = new Dictionary<Sentence, bool>();

            this.Errors = new List<CompilerError>();
        }

        public Dictionary<Sentence, bool> AtomicSentences { get; }

        public CSharpSyntaxNode DeclarationSyntax { get; set; }

        public string Code { get; set; }

        public Dictionary<(string, int), ConstantType> ConstantTypes => this.constantTypes.Value;

        public Dictionary<Sentence, bool> DatalogLiterals { get; }

        public Dictionary<Term, bool> DatalogTerms { get; }

        /// <summary>
        /// Gets the collection of errors that occurred during compilation.
        /// </summary>
        public IList<CompilerError> Errors { get; }

        public AssignedTypes AssignedTypes => this.assignedTypes.Value;

        public Dictionary<Expression, ImmutableHashSet<Variable>> ContainedVariables => this.containedVariables.Value;

        public KnowledgeBase KnowledgeBase { get; }

        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type resulting from compilation.
        /// </summary>
        public Type Type { get; set; }

        internal void AddCompilerError(Cursor cursor, System.Linq.Expressions.Expression<Func<string>> error, params object[] args)
        {
            var parts = ((System.Linq.Expressions.MemberExpression)error.Body).Member.Name.Split('_');
            var errorId = parts[0];

            bool? isWarning = null;
            switch (parts[1])
            {
                case "ERROR":
                    isWarning = false;
                    break;

                case "WARNING":
                    isWarning = true;
                    break;

#if DEBUG
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown error type '{0}'.", parts[1]), "error");
#endif
            }

            var errorFormat = error.Compile()();
            var errorText = string.Format(CultureInfo.CurrentCulture, errorFormat, args);
            this.Errors.Add(new CompilerError(cursor.FileName ?? string.Empty, cursor.Line, cursor.Column, errorId, errorText) { IsWarning = isWarning ?? true });
        }
    }
}
