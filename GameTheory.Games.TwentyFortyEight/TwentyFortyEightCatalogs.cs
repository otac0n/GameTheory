// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.TwentyFortyEight.TwentyFortyEightCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.TwentyFortyEight.TwentyFortyEightCatalogs.Players))]

namespace GameTheory.Games.TwentyFortyEight
{
    using GameTheory.Catalogs;

    internal static class TwentyFortyEightCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(TwentyFortyEightCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(TwentyFortyEightCatalogs).Assembly)
            {
            }
        }
    }
}
