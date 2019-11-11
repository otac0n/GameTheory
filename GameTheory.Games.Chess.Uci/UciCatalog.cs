// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Chess.Uci.UciCatalog))]

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using GameTheory.Catalogs;

    public class UciCatalog : PlayerCatalogBase
    {
        protected override IEnumerable<ICatalogPlayer> GetPlayers(Type gameStateType, Type moveType)
        {
            if (moveType != typeof(Move))
            {
                yield break;
            }

            foreach (var engine in Directory.EnumerateFiles("UciEngines", "*.exe"))
            {
                yield return new Player(engine);
            }
        }

        private class Player : ICatalogPlayer, IDisposable
        {
            private readonly string path;
            private UciEngine engine;

            public Player(string path)
            {
                this.path = path;
                this.engine = new UciEngine(this.path);
            }

            public Type GameStateType => typeof(GameState);

            public Type MoveType => typeof(Move);

            public string Name => this.engine.Name;

            public Type PlayerType => typeof(UciPlayer);

            public void Dispose()
            {
                this.engine?.Dispose();
            }
        }
    }
}
