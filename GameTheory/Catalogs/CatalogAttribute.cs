// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;

    /// <summary>
    /// Decorates an assembly allowing for the exposure of specific catalogs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class CatalogAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogAttribute"/> class.
        /// </summary>
        /// <param name="catalogType">The type of catalog being exposed.</param>
        /// <param name="implementationType">The catalog type to be instantiated.</param>
        public CatalogAttribute(Type catalogType, Type implementationType)
        {
            if (!catalogType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentOutOfRangeException(nameof(implementationType));
            }

            this.CatalogType = catalogType ?? throw new ArgumentNullException(nameof(catalogType));
            this.ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }

        /// <summary>
        /// Gets the type of catalog being exposed.
        /// </summary>
        public Type CatalogType { get; }

        /// <summary>
        /// Gets the catalog type to be instantiated.
        /// </summary>
        public Type ImplementationType { get; }
    }
}
