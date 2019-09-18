// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides enumeration for displays in an assembly.
    /// </summary>
    public class DisplayCatalog
    {
        private readonly ImmutableList<Assembly> assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for displays.</param>
        public DisplayCatalog(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to search for displays.</param>
        public DisplayCatalog(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = (assemblies ?? throw new ArgumentNullException(nameof(assemblies))).ToImmutableList();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Display> GetDisplays(Type gameStateType)
        {
            foreach (var assembly in this.assemblies)
            {
                var displays = (from t in assembly.ExportedTypes
                                where !t.GetTypeInfo().IsAbstract
                                where typeof(Display).IsAssignableFrom(t)
                                where t.GetConstructors().Any(c => c.GetParameters().Length == 0)
                                select t).ToArray();

                foreach (var d in displays)
                {
                    yield return (Display)Activator.CreateInstance(d);
                }
            }
        }
    }
}
