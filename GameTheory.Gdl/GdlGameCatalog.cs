namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GameTheory.Catalogs;

    public class GdlGameCatalog : GameTheory.Catalogs.GameCatalog
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

        protected override IEnumerable<Game> GetGames()
        {
            var patterns = this.searchPattern.Split('|');
            var paths = patterns
                .Aggregate(
                    Enumerable.Empty<string>(),
                    (acc, p) => acc.Concat(Directory.EnumerateFiles(this.folder, p, this.subFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)))
                .Distinct();

            foreach (var path in paths)
            {
                Game game;
                try
                {
                    var gdl = File.ReadAllText(path);
                    var compiler = new GameCompiler();
                    var result = compiler.Compile(gdl, path);
                    if (result.Errors.Any(e => !e.IsWarning))
                    {
                        continue;
                    }

                    game = new Game(result.Type);
                }
                catch (Exception)
                {
                    continue;
                }

                yield return game;
            }
        }
    }
}
