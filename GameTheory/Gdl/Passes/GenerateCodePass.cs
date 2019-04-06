// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class GenerateCodePass : CompilePass
    {
        public const string ErrorGeneratingCodeError = "GDL101";

        private static readonly Lazy<IList<string>> EarlierErrors = new Lazy<IList<string>>(() =>
        {
            return (from p in typeof(Resources).GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty)
                    let parts = p.Name.Split('_')
                    where parts.Length == 3
                    where parts[1] == "ERROR"
                    where string.Compare(parts[0], ErrorGeneratingCodeError, StringComparison.InvariantCulture) < 0
                    select parts[0])
                    .ToList()
                    .AsReadOnly();
        });

        /// <inheritdoc/>
        public override IList<string> BlockedByErrors => EarlierErrors.Value;

        /// <inheritdoc/>
        public override IList<string> ErrorsProduced => new[]
        {
            ErrorGeneratingCodeError,
        };

        /// <inheritdoc/>
        public override void Run(CompileResult result)
        {
            try
            {
                using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
                {
                    CodeGenerator.RenderGameState(result, stringWriter);
                    result.Code = stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                result.AddCompilerError(result.KnowledgeBase.StartCursor, () => Resources.GDL101_ERROR_ErrorGeneratingCode, ex.ToString());
            }
        }
    }
}
