// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.$game$.$game$Catalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.$game$.$game$Catalogs.Players))]

namespace GameTheory.Games.$game$
{
    using GameTheory.Catalogs;

    internal static class $game$Catalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof($game$Catalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof($game$Catalogs).Assembly)
            {
            }
        }
    }
}
