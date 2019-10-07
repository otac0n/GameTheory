// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.CenturySpiceRoad.Console.CenturySpiceRoadConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.CenturySpiceRoad.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class CenturySpiceRoadConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(CenturySpiceRoadConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
