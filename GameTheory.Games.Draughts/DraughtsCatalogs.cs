// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Draughts.DraughtsCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Draughts.DraughtsCatalogs.Players))]

namespace GameTheory.Games.Draughts
{
    using GameTheory.Catalogs;

    internal static class DraughtsCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(DraughtsCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(DraughtsCatalogs).Assembly)
            {
            }
        }
    }
}
