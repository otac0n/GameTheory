// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Skull.SkullCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Skull.SkullCatalogs.Players))]

namespace GameTheory.Games.Skull
{
    using GameTheory.Catalogs;

    internal static class SkullCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(SkullCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(SkullCatalogs).Assembly)
            {
            }
        }
    }
}
