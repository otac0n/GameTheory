// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Gdl.Passes
{
    using System.Linq;
    using GameTheory.Gdl;
    using GameTheory.Gdl.Passes;
    using NUnit.Framework;

    [TestFixture]
    public class ReportInconsistentConstantSemanticsPassTests
    {
        [TestCase("(<= (a 1) (b (a 1)))")]
        [TestCase("(<= (a 1) (b (a 1 2)))")]
        [TestCase("(<= (a 1 2) (b (a 1)))")]
        [TestCase("(<= (a (a 1)) (b 1))")]
        public void Run_WhenWhenKnowledgeBaseHasInconsistentSemantisForAConstant_YieldsError(string kb)
        {
            var compiler = new GameCompiler();

            var result = compiler.Compile(kb);

            Assert.IsNotEmpty(result.Errors.Where(e => e.ErrorNumber == ReportInconsistentConstantSemanticsPass.InconsistentConstantSemanticsError));
        }
    }
}
