// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Games.Hangman.HangmanCatalogs.Games))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Hangman.HangmanCatalogs.Players))]

namespace GameTheory.Games.Hangman
{
    using GameTheory.Catalogs;

    internal static class HangmanCatalogs
    {
        public class Games : AssemblyGameCatalog
        {
            public Games()
                : base(typeof(HangmanCatalogs).Assembly)
            {
            }
        }

        public class Players : AssemblyPlayerCatalog
        {
            public Players()
                : base(typeof(HangmanCatalogs).Assembly)
            {
            }
        }
    }
}
