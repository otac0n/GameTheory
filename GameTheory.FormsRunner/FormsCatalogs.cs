// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.FormsCatalogs.Players))]

namespace GameTheory
{
    using GameTheory.Catalogs;

    internal static class FormsCatalogs
    {
        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(FormsCatalogs).Assembly)
            {
            }
        }
    }
}
