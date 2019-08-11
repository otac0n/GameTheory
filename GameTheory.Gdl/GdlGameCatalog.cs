namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GameTheory.Catalogs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class GdlGameCatalog : GameCatalog
    {
        private readonly string folder;

        public GdlGameCatalog(string folder)
        {
            this.folder = folder;
        }

        protected override IEnumerable<IGame> GetGames() =>
            Directory.EnumerateFiles(this.folder, "METADATA", SearchOption.AllDirectories)
                .Distinct()
                .Select(path => new Game(path));

        private class Game : IGame
        {
            private readonly string gdlPath;
            private readonly Lazy<Type> gameStateType;
            private readonly Lazy<Type> moveType;

            public Game(string metadataPath)
            {
                var metadata = JsonConvert.DeserializeObject<GameMetadata>(File.ReadAllText(metadataPath));
                this.gdlPath = Path.Combine(Path.GetDirectoryName(metadataPath), metadata.RuleSheet);

                this.Name = metadata.GameName;
                this.gameStateType = new Lazy<Type>(
                    () =>
                    {
                        var gdl = File.ReadAllText(this.gdlPath);
                        var compiler = new GameCompiler();
                        var result = compiler.Compile(gdl, this.gdlPath);
                        return result.Type;
                    },
                    isThreadSafe: true);
                this.moveType = new Lazy<Type>(
                    () => Catalogs.Game.GetMoveType(this.GameStateType),
                    isThreadSafe: true);
            }

            /// <inheritdoc />
            public Type GameStateType => this.gameStateType.Value;

            /// <inheritdoc />
            public Type MoveType => this.moveType.Value;

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public override string ToString() => this.Name;
        }
    }
}
