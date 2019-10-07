// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.$game$.Console.$game$ConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.$game$.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class $game$ConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof($game$ConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
