// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Lotus.LotusCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Lotus.LotusCatalogs.Players))]

namespace GameTheory.Games.Lotus
{
    using GameTheory.Catalogs;

    internal static class LotusCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(LotusCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(LotusCatalogs).Assembly)
            {
            }
        }
    }
}
