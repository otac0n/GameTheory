// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides enumeration for games in an assembly.
    /// </summary>
    public class GameCatalog
    {
        /// <summary>
        /// Gets the default game catalog.
        /// </summary>
        public static readonly GameCatalog Default = new GameCatalog(typeof(IGameState<>).GetTypeInfo().Assembly);

        private readonly ImmutableList<Assembly> assemblies;
        private readonly Lazy<ImmutableList<Game>> availableGames;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for games</param>
        public GameCatalog(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for games</param>
        public GameCatalog(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = (assemblies ?? throw new ArgumentNullException(nameof(assemblies))).ToImmutableList();
            this.availableGames = new Lazy<ImmutableList<Game>>(() => this.GetGames().ToImmutableList(), isThreadSafe: true);
        }

        /// <summary>
        /// Gets the available games in the catalog.
        /// </summary>
        public IList<Game> AvailableGames => this.availableGames.Value;

        private IEnumerable<Game> GetGames()
        {
            foreach (var assembly in this.assemblies)
            {
                var games = (from t in assembly.ExportedTypes
                             let m = Game.GetMoveType(t)
                             where m != null
                             select new Game(t, m)).ToArray();

                foreach (var g in games)
                {
                    yield return g;
                }
            }
        }
    }
}
