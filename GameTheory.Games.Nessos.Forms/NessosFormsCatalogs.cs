// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.FormsRunner.Shared.Catalogs.IDisplayCatalog), typeof(GameTheory.Games.Nessos.Forms.NessosFormsCatalogs.Displays))]

namespace GameTheory.Games.Nessos.Forms
{
    using GameTheory.FormsRunner.Shared.Catalogs;

    internal static class NessosFormsCatalogs
    {
        public class Displays : AssemblyDisplayCatalog
        {
            public Displays()
                : base(typeof(GameState), typeof(NessosFormsCatalogs).Assembly)
            {
            }
        }
    }
}
