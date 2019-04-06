// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using Microsoft.CSharp;

    internal class CompileGeneratedCodePass : CompilePass
    {
        public const string ErrorComilingTypeError = "GDL102";

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
                var compiler = new CSharpCodeProvider();
                var options = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true,
                };
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Core.dll");
                options.ReferencedAssemblies.Add(typeof(IGameState<>).Assembly.Location);

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
                result.Type = assembly.GetType("GameState");
            }
            catch (Exception ex)
            {
                result.AddCompilerError(result.KnowledgeBase.StartCursor, () => Resources.GDL102_ERROR_ErrorComilingType, ex.ToString());
            }
        }
    }
}
