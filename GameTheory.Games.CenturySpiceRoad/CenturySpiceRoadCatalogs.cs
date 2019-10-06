// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.CenturySpiceRoad.CenturySpiceRoadCatalogs.ConsoleRenderers))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.CenturySpiceRoad.CenturySpiceRoadCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.CenturySpiceRoad.CenturySpiceRoadCatalogs.Players))]

namespace GameTheory.Games.CenturySpiceRoad
{
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class CenturySpiceRoadCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(CenturySpiceRoadCatalogs).Assembly)
            {
            }
        }

        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(CenturySpiceRoadCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(CenturySpiceRoadCatalogs).Assembly)
            {
            }
        }
    }
}
