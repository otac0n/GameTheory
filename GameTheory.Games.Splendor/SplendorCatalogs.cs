// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Splendor.SplendorCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Splendor.SplendorCatalogs.Players))]

namespace GameTheory.Games.Splendor
{
    using GameTheory.Catalogs;

    internal static class SplendorCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(SplendorCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(SplendorCatalogs).Assembly)
            {
            }
        }
    }
}
