// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.Nessos.Console.NessosConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.Nessos.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class NessosConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(NessosConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
