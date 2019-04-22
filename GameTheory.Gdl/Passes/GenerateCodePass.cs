// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;

    internal class GenerateCodePass : CompilePass
    {
        public const string ErrorGeneratingCodeError = "GDL102";

        /// <inheritdoc/>
        public override IList<string> BlockedByErrors => new[]
        {
            ConvertToCodeDomPass.ErrorConvertingToCodeDomError,
        };

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
                result.Code = result.DeclarationSyntax.ToFullString();
            }
            catch (Exception ex)
            {
                result.AddCompilerError(result.KnowledgeBase.StartCursor, () => Resources.GDL102_ERROR_ErrorGeneratingCode, ex.ToString());
            }
        }
    }
}
