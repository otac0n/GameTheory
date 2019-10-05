// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Nessos.NessosCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Nessos.NessosCatalogs.Players))]

namespace GameTheory.Games.Nessos
{
    using GameTheory.Catalogs;

    internal static class NessosCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(NessosCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(NessosCatalogs).Assembly)
            {
            }
        }
    }
}
