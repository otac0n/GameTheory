// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.PositivelyPerfectParfaitGame.PositivelyPerfectParfaitGameCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.PositivelyPerfectParfaitGame.PositivelyPerfectParfaitGameCatalogs.Players))]

namespace GameTheory.Games.PositivelyPerfectParfaitGame
{
    using GameTheory.Catalogs;

    internal static class PositivelyPerfectParfaitGameCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(PositivelyPerfectParfaitGameCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(PositivelyPerfectParfaitGameCatalogs).Assembly)
            {
            }
        }
    }
}
