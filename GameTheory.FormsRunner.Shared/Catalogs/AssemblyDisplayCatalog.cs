// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides enumeration for displays in an assembly.
    /// </summary>
    public class AssemblyDisplayCatalog : DisplayCatalogBase
    {
        private readonly Assembly assembly;
        private readonly Type baseGameStateType;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDisplayCatalog"/> class.
        /// </summary>
        /// <param name="baseGameStateType">The base game states supported by types in the assembly.</param>
        /// <param name="assembly">The assembly to search for displays.</param>
        public AssemblyDisplayCatalog(Type baseGameStateType, Assembly assembly)
        {
            this.baseGameStateType = baseGameStateType ?? throw new ArgumentNullException(nameof(baseGameStateType));
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <inheritdoc/>
        protected override IEnumerable<Type> GetDisplays(Type gameStateType)
        {
            if (this.baseGameStateType.IsAssignableFrom(gameStateType))
            {
                var displays = (from t in this.assembly.ExportedTypes
                                where !t.GetTypeInfo().IsAbstract
                                where typeof(Display).IsAssignableFrom(t)
                                where t.GetConstructors().Any(c => c.GetParameters().Length == 0)
                                select t).ToArray();

                foreach (var d in displays)
                {
                    yield return d;
                }
            }
        }
    }
}
