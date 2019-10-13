// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.FormsRunner.Shared.Catalogs.IDisplayCatalog), typeof(GameTheory.Games.Chess.Forms.ChessFormsCatalogs.Displays))]

namespace GameTheory.Games.Chess.Forms
{
    using GameTheory.FormsRunner.Shared.Catalogs;

    internal static class ChessFormsCatalogs
    {
        public class Displays : AssemblyDisplayCatalog
        {
            public Displays()
                : base(typeof(GameState), typeof(ChessFormsCatalogs).Assembly)
            {
            }
        }
    }
}
