// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

[assembly: GameTheory.Catalogs.Catalog(typeof(GameTheory.Catalogs.IPlayerCatalog), typeof(GameTheory.Games.Chess.Uci.UciCatalog))]

namespace GameTheory.Games.Chess.Uci
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Catalogs;

    public class UciCatalog : PlayerCatalogBase
    {
        protected override IEnumerable<ICatalogPlayer> GetPlayers(Type gameStateType, Type moveType)
        {
            if (gameStateType != typeof(GameState) || moveType != typeof(Move))
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
            private readonly UciEngine engine;
            private readonly Lazy<IReadOnlyList<Initializer>> initializers;
            private readonly string path;

            public Player(string path)
            {
                this.path = path;
                this.engine = new UciEngine(this.path);
                this.initializers = new Lazy<IReadOnlyList<Initializer>>(() =>
                {
                    return new ReadOnlyCollection<Initializer>(new[]
                    {
                        new Initializer(SharedResources.DefaultInstance, parameters => new UciPlayer((PlayerToken)parameters[0], this.path), new[]
                        {
                            new DynamicParameterInfo("playerToken", typeof(PlayerToken), 0, ParameterAttributes.None, false, null, null),
                        }),
                    });
                });
            }

            public Type GameStateType => typeof(GameState);

            public IReadOnlyList<Initializer> Initializers => this.initializers.Value;

            public Type MoveType => typeof(Move);

            public string Name => this.engine.Name;

            public Type PlayerType => typeof(UciPlayer);

            public void Dispose()
            {
                this.engine.Dispose();
            }
        }
    }
}
