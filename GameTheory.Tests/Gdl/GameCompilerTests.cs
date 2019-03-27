// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Tests.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using GameTheory.Gdl;
    using NUnit.Framework;

    [TestFixture]
    public class GameCompilerTests
    {
        [Test]
        public void Compile_WhenGivenTicTacToe_ReturnsAGameOfTicTacToe()
        {
            var gdl = LoadAssemblyResource("GameTheory.Tests.Gdl.TicTacToe.gdl");
            var compiler = new GameCompiler();
            var result = compiler.Compile(gdl);
            Assert.NotNull(result.Type);
        }

        private static string LoadAssemblyResource(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
