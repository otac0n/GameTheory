// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using GameTheory.Gdl.Shared;
    using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

    internal class CompileGeneratedCodePass : CompilePass
    {
        public const string ErrorComilingTypeError = "GDL103";

        public override IList<string> BlockedByErrors => new[]
        {
            GenerateCodePass.ErrorGeneratingCodeError,
        };

        public override IList<string> ErrorsProduced => new[]
        {
            ErrorComilingTypeError,
        };

        public override void Run(CompileResult result)
        {
            try
            {
                var compiler = new CSharpCodeProvider(new CompilerSettings());
                var options = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true,
                };
                options.ReferencedAssemblies.Add(typeof(ISet<>).Assembly.Location);
                options.ReferencedAssemblies.Add(typeof(IReadOnlyCollection<>).Assembly.Location);
                options.ReferencedAssemblies.Add(typeof(Enumerable).Assembly.Location);
                options.ReferencedAssemblies.Add(typeof(XmlWriter).Assembly.Location);
                options.ReferencedAssemblies.Add(typeof(IGameState<>).Assembly.Location);
                options.ReferencedAssemblies.Add(typeof(IXml).Assembly.Location);

                var results = compiler.CompileAssemblyFromSource(options, result.Code);
                if (results.Errors.HasErrors)
                {
                    foreach (CompilerError error in results.Errors)
                    {
                        result.Errors.Add(error);
                    }

                    return;
                }

                var assembly = results.CompiledAssembly;
                result.Type = assembly.GetType($"{result.Name}.GameState");
            }
            catch (Exception ex)
            {
                result.AddCompilerError(result.KnowledgeBase.StartCursor, () => Resources.GDL103_ERROR_ErrorComilingType, ex.ToString());
            }
        }

        private class CompilerSettings : ICompilerSettings
        {
            private static readonly string CompilerPathSuffix = Path.Combine("roslyn", "csc.exe");

            /// <inheritdoc />
            public string CompilerFullPath
            {
                get
                {
                    var path = CompilerSearchPaths.Select(p => Path.Combine(p, CompilerPathSuffix)).Where(File.Exists).FirstOrDefault();
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new FileNotFoundException("Could not locate csc.exe");
                    }

                    return path;
                }
            }

            /// <inheritdoc />
            public int CompilerServerTimeToLive => 5;

            private static string[] CompilerSearchPaths => new[]
            {
                Environment.CurrentDirectory,
                AppContext.BaseDirectory,
                Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
            };
        }
    }
}
