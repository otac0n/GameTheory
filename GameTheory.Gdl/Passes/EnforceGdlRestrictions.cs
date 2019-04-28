// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Passes
{
    using System.Collections.Generic;
    using KnowledgeInterchangeFormat.Expressions;

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
            new GdlEnforcementWalker(result).Walk((Expression)result.KnowledgeBase);
        }

        private class GdlEnforcementWalker : SupportedExpressionsTreeWalker
        {
            private readonly CompileResult result;
            private RulePart rulePart;

            public GdlEnforcementWalker(CompileResult result)
            {
                this.result = result;
            }

            private enum RulePart
            {
                Atomic,
                Head,
                Body,
            }

            public override void Walk(KnowledgeBase knowledgeBase)
            {
                foreach (var form in knowledgeBase.Forms)
                {
                    this.rulePart = RulePart.Atomic;
                    this.Walk((Expression)form);
                }
            }

            public override void Walk(Implication implication)
            {
                this.rulePart = RulePart.Head;
                this.Walk((Expression)implication.Consequent);

                this.rulePart = RulePart.Body;
                foreach (var sentence in implication.Antecedents)
                {
                    this.Walk((Expression)sentence);
                }
            }

            public override void Walk(ImplicitRelationalSentence implicitRelationalSentence)
            {
                var key = (implicitRelationalSentence.Relation, implicitRelationalSentence.Arguments.Count);
                if (object.Equals(key, KnownConstants.Role))
                {
                    if (this.rulePart == RulePart.Head || (this.rulePart == RulePart.Atomic && this.result.ContainedVariables[implicitRelationalSentence].Count > 0))
                    {
                        this.result.AddCompilerError(implicitRelationalSentence.StartCursor, () => Resources.GDL008_ERROR_RoleRelationUsedInRule);
                    }
                }
                else if (object.Equals(key, KnownConstants.Init))
                {
                    if (this.rulePart == RulePart.Body)
                    {
                        this.result.AddCompilerError(implicitRelationalSentence.StartCursor, () => Resources.GDL009_ERROR_InitRelationUsedInRuleBody);
                    }
                }
                else if (object.Equals(key, KnownConstants.True))
                {
                    if (this.rulePart != RulePart.Body)
                    {
                        this.result.AddCompilerError(implicitRelationalSentence.StartCursor, () => Resources.GDL011_ERROR_TrueRelationUsedOutsideRuleBody);
                    }
                }
                else if (object.Equals(key, KnownConstants.Next))
                {
                    if (this.rulePart != RulePart.Head)
                    {
                        this.result.AddCompilerError(implicitRelationalSentence.StartCursor, () => Resources.GDL012_ERROR_NextRelationUsedOutsideRuleHead);
                    }
                }
                else if (object.Equals(key, KnownConstants.Does))
                {
                    if (this.rulePart != RulePart.Body)
                    {
                        this.result.AddCompilerError(implicitRelationalSentence.StartCursor, () => Resources.GDL013_ERROR_DoesUsedOutsideRuleBody);
                    }
                }
            }
        }
    }
}
