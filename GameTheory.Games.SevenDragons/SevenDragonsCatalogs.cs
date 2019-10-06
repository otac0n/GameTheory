// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.SevenDragons.SevenDragonsCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.SevenDragons.SevenDragonsCatalogs.Players))]

namespace GameTheory.Games.SevenDragons
{
    using GameTheory.Catalogs;

    internal static class SevenDragonsCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(SevenDragonsCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(SevenDragonsCatalogs).Assembly)
            {
            }
        }
    }
}
