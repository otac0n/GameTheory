// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.Ergo.Console.ErgoConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.Ergo.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class ErgoConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(ErgoConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
