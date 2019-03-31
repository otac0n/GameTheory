namespace GameTheory.Gdl
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using KnowledgeInterchangeFormat;
    using KnowledgeInterchangeFormat.Expressions;
    using Pegasus.Common;

    public class GameCompiler
    {
        private static readonly IList<Type> PassTypes = typeof(GameCompiler).GetTypeInfo().Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(CompilePass)))
            .ToList()
            .AsReadOnly();

        public CompileResult Compile(string game, string fileName = null)
        {
            KnowledgeBase knowledgeBase;
            CompileResult result;
            try
            {
                knowledgeBase = new KifParser().Parse(game ?? string.Empty, fileName);
            }
            catch (FormatException ex)
            {
                var cursor = ex.Data["cursor"] as Cursor;
                if (cursor != null && Regex.IsMatch(ex.Message, @"^[A-Z]{2,4}\d+:"))
                {
                    var parts = ex.Message.Split(new[] { ':' }, 2);
                    result = new CompileResult(null);
                    result.Errors.Add(new CompilerError(cursor.FileName ?? string.Empty, cursor.Line, cursor.Column, parts[0], parts[1]));
                    return result;
                }

                throw;
            }

            result = new CompileResult(knowledgeBase);

            var passes = PassTypes.Select(t => (CompilePass)Activator.CreateInstance(t)).ToList();
            while (true)
            {
                var existingErrors = new HashSet<string>(result.Errors.Where(e => !e.IsWarning).Select(e => e.ErrorNumber));
                var pendingErrors = new HashSet<string>(passes.SelectMany(p => p.ErrorsProduced));

                var nextPasses = passes
                    .Where(p => !p.BlockedByErrors.Any(e => existingErrors.Contains(e)))
                    .Where(p => !p.BlockedByErrors.Any(e => pendingErrors.Contains(e)))
                    .ToList();

                if (nextPasses.Count == 0)
                {
                    break;
                }

                foreach (var pass in nextPasses)
                {
                    pass.Run(result);
                    passes.Remove(pass);
                }
            }

            return result;
        }
    }
}
