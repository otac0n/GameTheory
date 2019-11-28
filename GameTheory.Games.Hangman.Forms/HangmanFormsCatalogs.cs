// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.FormsRunner.Shared.Catalogs.IDisplayCatalog), typeof(GameTheory.Games.Hangman.Forms.HangmanFormsCatalogs.Displays))]

namespace GameTheory.Games.Hangman.Forms
{
    using GameTheory.FormsRunner.Shared.Catalogs;

    internal static class HangmanFormsCatalogs
    {
        public class Displays : AssemblyDisplayCatalog
        {
            public Displays()
                : base(typeof(GameState), typeof(HangmanFormsCatalogs).Assembly)
            {
            }
        }
    }
}
