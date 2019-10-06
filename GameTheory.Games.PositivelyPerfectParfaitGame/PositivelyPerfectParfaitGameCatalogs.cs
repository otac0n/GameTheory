// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.PositivelyPerfectParfaitGame.PositivelyPerfectParfaitGameCatalogs.ConsoleRenderers))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.PositivelyPerfectParfaitGame.PositivelyPerfectParfaitGameCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.PositivelyPerfectParfaitGame.PositivelyPerfectParfaitGameCatalogs.Players))]

namespace GameTheory.Games.PositivelyPerfectParfaitGame
{
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class PositivelyPerfectParfaitGameCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(PositivelyPerfectParfaitGameCatalogs).Assembly)
            {
            }
        }

        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(PositivelyPerfectParfaitGameCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(PositivelyPerfectParfaitGameCatalogs).Assembly)
            {
            }
        }
    }
}
