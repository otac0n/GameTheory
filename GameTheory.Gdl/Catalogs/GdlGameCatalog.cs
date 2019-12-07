// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IGameCatalog), typeof(GameTheory.Gdl.Catalogs.GdlGameCatalog))]
[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Gdl.Catalogs.GdlGameCatalog))]

namespace GameTheory.Gdl.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GameTheory.Catalogs;

    public class GdlGameCatalog : GameCatalogBase, IPlayerCatalog
    {
        private readonly string folder;

        public GdlGameCatalog(string folder = null)
        {
            this.folder = folder ?? Path.Combine(Environment.CurrentDirectory, "Games");
        }

        public IReadOnlyList<ICatalogPlayer> FindPlayers(Type gameStateType, Type moveType)
        {
            var list = new List<ICatalogPlayer>();
            foreach (var game in this.AvailableGames.Cast<GdlGame>())
            {
                if (game.IsGameStateType(gameStateType))
                {
                    list.Add(game);
                }
            }

            return list;
        }

        protected override IEnumerable<ICatalogGame> GetGames() =>
            Directory.EnumerateFiles(this.folder, "METADATA", SearchOption.AllDirectories)
                .Distinct()
                .Select(path => new GdlGame(path));
    }
}
