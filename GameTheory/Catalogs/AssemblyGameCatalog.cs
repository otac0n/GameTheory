// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Exposes the games available as exported types from a list of assemblies.
    /// </summary>
    public class AssemblyGameCatalog : GameCatalog
    {
        private readonly ImmutableList<Assembly> assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyGameCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for games.</param>
        public AssemblyGameCatalog(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyGameCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for games.</param>
        public AssemblyGameCatalog(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = (assemblies ?? throw new ArgumentNullException(nameof(assemblies))).ToImmutableList();
        }

        /// <inheritdoc/>
        protected override IEnumerable<IGame> GetGames()
        {
            foreach (var assembly in this.assemblies)
            {
                var games = (from t in assembly.ExportedTypes
                             where !t.GetTypeInfo().IsAbstract
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
