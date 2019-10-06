// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.ConsoleRunner.Shared.Catalogs.IConsoleRendererCatalog), typeof(GameTheory.Games.Chess.Console.ChessConsoleCatalogs.ConsoleRenderers))]

namespace GameTheory.Games.Chess.Console
{
    using GameTheory.ConsoleRunner.Shared.Catalogs;

    internal static class ChessConsoleCatalogs
    {
        public class ConsoleRenderers : AssemblyConsoleRendererCatalog
        {
            public ConsoleRenderers()
                : base(typeof(ChessConsoleCatalogs).Assembly)
            {
            }
        }
    }
}
