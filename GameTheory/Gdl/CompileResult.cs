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
            this.Errors = new List<CompilerError>();
        }

        /// <summary>
        /// Gets or sets the type resulting from compilation.
        /// </summary>
        public Type Type { get; set; }

        public KnowledgeBase KnowledgeBase { get; }

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
