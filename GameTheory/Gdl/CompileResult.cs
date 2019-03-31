// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Globalization;
    using KnowledgeInterchangeFormat.Expressions;
    using Pegasus.Common;

    public class CompileResult
    {
        public CompileResult(KnowledgeBase knowledgeBase)
        {
            this.KnowledgeBase = knowledgeBase;
            this.ConstantTypes = new Dictionary<string, ConstantType>
            {
                { "ROLE", ConstantType.Relation },
                { "INIT", ConstantType.Relation },
                { "TRUE", ConstantType.Relation },
                { "DOES", ConstantType.Relation },
                { "NEXT", ConstantType.Relation },
                { "LEGAL", ConstantType.Relation },
                { "GOAL", ConstantType.Relation },
                { "TERMINAL", ConstantType.Logical },
                { "DISTINCT", ConstantType.Relation },
            };

            for (var i = 0; i <= 100; i++)
            {
                this.ConstantTypes[i.ToString()] = ConstantType.Object;
            }

            this.ConstantArities = new Dictionary<string, int>
            {
                { "ROLE", 1 },
                { "INIT", 1 },
                { "TRUE", 1 },
                { "DOES", 2 },
                { "NEXT", 1 },
                { "LEGAL", 2 },
                { "GOAL", 2 },
                { "DISTINCT", 2 },
            };

            this.AtomicSentences = new Dictionary<Sentence, bool>();
            this.DatalogTerms = new Dictionary<Term, bool>();
            this.DatalogRules = new Dictionary<Implication, bool>();
            this.DatalogLiterals = new Dictionary<Sentence, bool>();
            this.GroundExpressions = new Dictionary<Expression, bool>();

            this.Errors = new List<CompilerError>();
        }

        /// <summary>
        /// Gets or sets the type resulting from compilation.
        /// </summary>
        public Type Type { get; set; }

        public KnowledgeBase KnowledgeBase { get; }

        public Dictionary<string, ConstantType> ConstantTypes { get; }

        public Dictionary<string, int> ConstantArities { get; }

        public Dictionary<Sentence, bool> AtomicSentences { get; }

        public Dictionary<Term, bool> DatalogTerms { get; }

        public Dictionary<Implication, bool> DatalogRules { get; }

        public Dictionary<Sentence, bool> DatalogLiterals { get; }

        public Dictionary<Expression, bool> GroundExpressions { get; }

        /// <summary>
        /// Gets the collection of errors that occurred during compilation.
        /// </summary>
        public IList<CompilerError> Errors { get; }

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
