// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.FiveTribes.Console.FiveTribesConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.FiveTribes.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class FiveTribesConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(FiveTribesConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
