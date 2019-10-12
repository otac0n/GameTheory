// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.LoveLetter.LoveLetterCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.LoveLetter.LoveLetterCatalogs.Players))]

namespace GameTheory.Games.LoveLetter
{
    using GameTheory.Catalogs;

    internal static class LoveLetterCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(LoveLetterCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(LoveLetterCatalogs).Assembly)
            {
            }
        }
    }
}
