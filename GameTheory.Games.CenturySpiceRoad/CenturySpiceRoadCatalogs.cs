// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.CenturySpiceRoad.CenturySpiceRoadCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.CenturySpiceRoad.CenturySpiceRoadCatalogs.Players))]

namespace GameTheory.Games.CenturySpiceRoad
{
    using GameTheory.Catalogs;

    internal static class CenturySpiceRoadCatalogs
    {
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
