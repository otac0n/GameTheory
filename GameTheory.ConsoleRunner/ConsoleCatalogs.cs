// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.ConsoleCatalogs.ConsoleRenderers))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.ConsoleCatalogs.Players))]

namespace GameTheory
{
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class ConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(ConsoleCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(ConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
