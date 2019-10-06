// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.NormalFormGame.NormalFormGameCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.NormalFormGame.NormalFormGameCatalogs.Players))]

namespace GameTheory.Games.NormalFormGame
{
    using GameTheory.Catalogs;

    internal static class NormalFormGameCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(NormalFormGameCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(NormalFormGameCatalogs).Assembly)
            {
            }
        }
    }
}
