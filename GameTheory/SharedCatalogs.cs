// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.SharedCatalogs.Players))]

namespace GameTheory
{
    using GameTheory.Catalogs;

    internal static class SharedCatalogs
    {
        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(SharedCatalogs).Assembly)
            {
            }
        }
    }
}
