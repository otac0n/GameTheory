// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Gdl.Passes
{
    using System.Linq;
    using GameTheory.Gdl;
    using GameTheory.Gdl.Passes;
    using NUnit.Framework;

    [TestFixture]
    public class ReportInconsistentArityPassTest
    {
        [TestCase("(<= (a 1) (a 1 2))")]
        [TestCase("(<= (a 1) b (a 1 2))")]
        [TestCase("(<= (a 1 2) (a 1))")]
        [TestCase("(<= (a 1) (b 1) (b 1 2))")]
        [TestCase("(<= (a 1) (a (b 1)) (a (b 1 2)))")]
        public void Run_WhenWhenKnowledgeBaseHasInconsistentArity_YieldsError(string kb)
        {
            var compiler = new GameCompiler();

            var result = compiler.Compile(kb);

            Assert.IsNotEmpty(result.Errors.Where(e => e.ErrorNumber == ReportInconsistentArityPass.InconsistentArityError));
        }

        [TestCase("(<= (a @etc) (a 1))")]
        [TestCase("(<= (a 1 @etc) (a 1))")]
        [TestCase("(<= (a @etc) (a 1 2))")]
        [TestCase("(<= (a 1 @etc) (a 1 2))")]
        [TestCase("(<= (a 1) (a @etc))")]
        [TestCase("(<= (a 1) (a 1 @etc))")]
        [TestCase("(<= (a 1 2) (a @etc))")]
        [TestCase("(<= (a 1 2) (a 1 @etc))")]
        public void Run_WhenWhenKnowledgeBaseHasSequenceVariables_YieldsError(string kb)
        {
            var compiler = new GameCompiler();

            var result = compiler.Compile(kb);

            Assert.IsNotEmpty(result.Errors.Where(e => e.ErrorNumber == ReportInconsistentArityPass.SequenceVariablesUnsupportedError));
        }
    }
}
