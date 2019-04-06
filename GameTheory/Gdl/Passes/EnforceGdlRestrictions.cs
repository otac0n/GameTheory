// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System.Collections.Generic;

    internal class EnforceGdlRestrictions : CompilePass
    {
        public const string RoleRelationUsedInRuleError = "GDL008";
        public const string InitRelationUsedInRuleBodyError = "GDL009";
        public const string InitRelationDependencyError = "GDL010";
        public const string TrueRelationUsedOutsideRuleBodyError = "GDL011";
        public const string NextRelationUsedOutsideRuleHeadError = "GDL012";
        public const string DoesUsedOutsideRuleBodyError = "GDL013";
        public const string DoesRelationDependencyError = "GDL014";

        public override IList<string> BlockedByErrors => new[]
        {
            EnforceDatalogRuleFormatPass.RuleHeadMustBeAtomicError,
            EnforceDatalogRuleFormatPass.RuleBodyMustBeAtomicError,
            EnforceDatalogRuleFormatPass.RecursionResrictionError,
        };

        public override IList<string> ErrorsProduced => new[]
        {
            RoleRelationUsedInRuleError,
            InitRelationUsedInRuleBodyError,
            InitRelationDependencyError,
            TrueRelationUsedOutsideRuleBodyError,
            NextRelationUsedOutsideRuleHeadError,
            DoesUsedOutsideRuleBodyError,
            DoesRelationDependencyError,
        };

        public override void Run(CompileResult result)
        {
        }
    }
}
