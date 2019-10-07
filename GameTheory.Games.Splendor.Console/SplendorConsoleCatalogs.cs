// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.Splendor.Console.SplendorConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.Splendor.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class SplendorConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(SplendorConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
