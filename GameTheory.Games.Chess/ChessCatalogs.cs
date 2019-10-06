// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.Chess.ChessCatalogs.ConsoleRenderers))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Chess.ChessCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Chess.ChessCatalogs.Players))]

namespace GameTheory.Games.Chess
{
    using GameTheory.Catalogs;
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class ChessCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(ChessCatalogs).Assembly)
            {
            }
        }

        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(ChessCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(ChessCatalogs).Assembly)
            {
            }
        }
    }
}
