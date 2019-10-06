// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.PositivelyPerfectParfaitGame.Console.PositivelyPerfectParfaitGameConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.PositivelyPerfectParfaitGame.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class PositivelyPerfectParfaitGameConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(PositivelyPerfectParfaitGameConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
