namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GameTheory.Catalogs;

    public class GdlGameCatalog : GameCatalog
    {
        private readonly string folder;
        private readonly string searchPattern;
        private readonly bool subFolders;

        public GdlGameCatalog(string folder, string searchPattern = "*.gdl|*.kif", bool subFolders = true)
        {
            this.folder = folder;
            this.searchPattern = searchPattern;
            this.subFolders = subFolders;
        }

        protected override IEnumerable<IGame> GetGames()
        {
            var patterns = this.searchPattern.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            return patterns
                .Aggregate(
                    Enumerable.Empty<string>(),
                    (acc, p) => acc.Concat(Directory.EnumerateFiles(this.folder, p, this.subFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)))
                .Distinct()
                .Select(path => new Game(path));
        }

        private class Game : IGame
        {
            private readonly string path;
            private readonly Lazy<Type> gameStateType;
            private readonly Lazy<Type> moveType;

            public Game(string path)
            {
                this.path = path;
                this.Name = Path.GetFileName(path);
                this.gameStateType = new Lazy<Type>(
                    () =>
                    {
                        var gdl = File.ReadAllText(this.path);
                        var compiler = new GameCompiler();
                        var result = compiler.Compile(gdl, this.path);
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
