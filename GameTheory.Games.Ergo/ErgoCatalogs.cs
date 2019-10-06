// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Ergo.ErgoCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Ergo.ErgoCatalogs.Players))]

namespace GameTheory.Games.Ergo
{
    using GameTheory.Catalogs;

    internal static class ErgoCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(ErgoCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(ErgoCatalogs).Assembly)
            {
            }
        }
    }
}
