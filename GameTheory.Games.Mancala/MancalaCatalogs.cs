// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Mancala.MancalaCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Mancala.MancalaCatalogs.Players))]

namespace GameTheory.Games.Mancala
{
    using GameTheory.Catalogs;

    internal static class MancalaCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(MancalaCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(MancalaCatalogs).Assembly)
            {
            }
        }
    }
}
