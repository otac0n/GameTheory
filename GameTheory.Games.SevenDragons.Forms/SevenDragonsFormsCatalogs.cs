// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.FormsRunner.Shared.Catalogs.IDisplayCatalog), typeof(GameTheory.Games.SevenDragons.Forms.SevenDragonsFormsCatalogs.Displays))]

namespace GameTheory.Games.SevenDragons.Forms
{
    using GameTheory.FormsRunner.Shared.Catalogs;

    internal static class SevenDragonsFormsCatalogs
    {
        public class Displays : AssemblyDisplayCatalog
        {
            public Displays()
                : base(typeof(GameState), typeof(SevenDragonsFormsCatalogs).Assembly)
            {
            }
        }
    }
}
