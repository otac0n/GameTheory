// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Chess.Uci.Catalogs.UciCatalog))]

namespace GameTheory.Games.Chess.Uci.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GameTheory.Catalogs;

    public class UciCatalog : PlayerCatalogBase
    {
        private readonly string folder;

        public UciCatalog(string folder = null)
        {
            this.folder = folder ?? Path.Combine(Environment.CurrentDirectory, "UciEngines");
        }

        /// <inheritdoc/>
        protected override IEnumerable<ICatalogPlayer> GetPlayers(Type gameStateType, Type moveType) =>
            gameStateType != typeof(GameState) || moveType != typeof(Move)
                ? Enumerable.Empty<UciCatalogPlayer>()
                : Directory.EnumerateFiles(this.folder, "METADATA", SearchOption.AllDirectories)
                    .Distinct()
                    .Select(path => new UciCatalogPlayer(path));
    }
}
