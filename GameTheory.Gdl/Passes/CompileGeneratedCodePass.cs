// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using GameTheory.Gdl.Shared;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

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
                var dotnetPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

                var compilation = CSharpCompilation.Create(
                    result.Name,
                    new[]
                    {
                        CSharpSyntaxTree.ParseText(result.Code),
                    },
                    new[]
                    {
                        MetadataReference.CreateFromFile(Path.Combine(dotnetPath, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(dotnetPath, "System.Xml.ReaderWriter.dll")),
                        MetadataReference.CreateFromFile(typeof(XmlWriter).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(IReadOnlyCollection<>).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(IGameState<>).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(IXml).Assembly.Location),
                    },
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using var assemblyStream = new MemoryStream();
                var results = compilation.Emit(assemblyStream);

                foreach (var diagnostic in results.Diagnostics)
                {
                    var lineSpan = diagnostic.Location.SourceTree.GetLineSpan(diagnostic.Location.SourceSpan);

                    result.Errors.Add(new CompilerError
                    {
                        ErrorNumber = diagnostic.Id,
                        ErrorText = diagnostic.GetMessage(CultureInfo.CurrentCulture),
                        IsWarning = diagnostic.Severity != DiagnosticSeverity.Error,
                        FileName = diagnostic.Location.SourceTree.FilePath,
                        Line = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                    });
                }

                if (!results.Success)
                {
                    return;
                }

                var assembly = Assembly.Load(assemblyStream.ToArray());

                result.Type = assembly.GetType($"{result.Name}.GameState");
                result.Player = assembly.GetType($"{result.Name}.{result.NamespaceScope.GetPublic(result.Name + "MaximizingPlayer")}");
            }
            catch (Exception ex)
            {
                result.AddCompilerError(result.KnowledgeBase.StartCursor, () => Resources.GDL103_ERROR_ErrorComilingType, ex.ToString());
            }
        }
    }
}
