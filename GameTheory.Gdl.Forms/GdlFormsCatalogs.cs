// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.FormsRunner.Shared.Catalogs.IDisplayCatalog), typeof(GameTheory.Games.TwentyFortyEight.Forms.GdlFormsCatalogs.Displays))]

namespace GameTheory.Games.TwentyFortyEight.Forms
{
    using System;
    using System.Collections.Generic;
    using GameTheory.FormsRunner.Shared.Catalogs;
    using GameTheory.FormsRunner.Shared.Displays;

    internal static class GdlFormsCatalogs
    {
        public class Displays : IDisplayCatalog
        {
            public IReadOnlyList<Type> FindDisplays(Type gameStateType) => new[]
            {
                typeof(IXmlWithStylesheetDisplay),
            };
        }
    }
}
