// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.FiveTribes.FiveTribesCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.FiveTribes.FiveTribesCatalogs.Players))]

namespace GameTheory.Games.FiveTribes
{
    using GameTheory.Catalogs;

    internal static class FiveTribesCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(FiveTribesCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(FiveTribesCatalogs).Assembly)
            {
            }
        }
    }
}
