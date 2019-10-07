// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Providse plugin capabilities.
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// Loads the catalogs of the specified types from the specified path.
        /// </summary>
        /// <typeparam name="T">The type of catalogs to load.</typeparam>
        /// <param name="makeComposite">A function used to combine multiple catalogs into a single composite catalog.</param>
        /// <param name="path">The path to search. If <c>null</c>, <see cref="Environment.CurrentDirectory"/> will be used.</param>
        /// <returns>The requested catalogs.</returns>
        public static T LoadCatalogs<T>(Func<T[], T> makeComposite, string path = null)
        {
            if (path == null)
            {
                path = Environment.CurrentDirectory;
            }

            var libraries = Enumerable.Concat(
                Directory.EnumerateFiles(path, "GameTheory.*.dll"),
                Directory.EnumerateFiles(path, "GameTheory.*.exe"));

            var assemblies = new List<Assembly>
            {
                typeof(SharedCatalogs).Assembly,
            };
            foreach (var library in libraries)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(library));
                }
                catch
                {
                    Debugger.Break();
                    throw;
                }
            }

            var catalogs = new List<CatalogAttribute>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var attributes = assembly.GetCustomAttributes<CatalogAttribute>();
                    foreach (var attribute in attributes)
                    {
                        if (typeof(T).IsAssignableFrom(attribute.CatalogType))
                        {
                            catalogs.Add(attribute);
                        }
                    }
                }
                catch
                {
                    Debugger.Break();
                    throw;
                }
            }

            var instances = new List<T>();
            foreach (var catalog in catalogs)
            {
                try
                {
                    instances.Add((T)Activator.CreateInstance(catalog.ImplementationType));
                }
                catch
                {
                    Debugger.Break();
                    throw;
                }
            }

            return makeComposite(instances.ToArray());
        }

        /// <summary>
        /// Loads game catalogs from the specified path.
        /// </summary>
        /// <param name="path">The path to search. If <c>null</c>, <see cref="Environment.CurrentDirectory"/> will be used.</param>
        /// <returns>The requested catalogs.</returns>
        public static IGameCatalog LoadGameCatalogs(string path = null) =>
            LoadCatalogs<IGameCatalog>(c => new CompositeGameCatalog(c), path);

        /// <summary>
        /// Loads player catalogs from the specified path.
        /// </summary>
        /// <param name="path">The path to search. If <c>null</c>, <see cref="Environment.CurrentDirectory"/> will be used.</param>
        /// <returns>The requested catalogs.</returns>
        public static IPlayerCatalog LoadPlayerCatalogs(string path = null) =>
            LoadCatalogs<IPlayerCatalog>(p => new CompositePlayerCatalog(p), path);
    }
}
