// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System;
    using System.Collections.Generic;

    internal class AssignTypesPass : CompilePass
    {
        public override IList<string> BlockedByErrors => new[]
        {
            EnforceGdlRestrictions.RoleRelationUsedInRuleError,
            EnforceGdlRestrictions.InitRelationUsedInRuleBodyError,
            EnforceGdlRestrictions.InitRelationDependencyError,
            EnforceGdlRestrictions.TrueRelationUsedOutsideRuleBodyError,
            EnforceGdlRestrictions.NextRelationUsedOutsideRuleHeadError,
            EnforceGdlRestrictions.DoesUsedOutsideRuleBodyError,
            EnforceGdlRestrictions.DoesRelationDependencyError,
        };

        public override IList<string> ErrorsProduced => Array.Empty<string>();

        public override void Run(CompileResult result)
        {
            var types = result.ExpressionTypes;
        }
    }
}
