// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.FormsRunner.Shared.Catalogs.IDisplayCatalog), typeof(GameTheory.Games.$game$.Forms.$game$FormsCatalogs.Displays))]

namespace GameTheory.Games.$game$.Forms
{
    using GameTheory.FormsRunner.Shared.Catalogs;

    internal static class $game$FormsCatalogs
    {
        public class Displays : AssemblyDisplayCatalog
        {
            public Displays()
                : base(typeof(GameState), typeof($game$FormsCatalogs).Assembly)
            {
            }
        }
    }
}
