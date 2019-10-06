// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.TicTacToe.TicTacToeCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.TicTacToe.TicTacToeCatalogs.Players))]

namespace GameTheory.Games.TicTacToe
{
    using GameTheory.Catalogs;

    internal static class TicTacToeCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(TicTacToeCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(TicTacToeCatalogs).Assembly)
            {
            }
        }
    }
}
