// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A convenience class implementing <see cref="IInitializer"/>.
    /// </summary>
    public class Initializer : IInitializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Initializer"/> class.
        /// </summary>
        /// <param name="name">The name of the initializer.</param>
        /// <param name="accessor">The accessor function that will create new instances of the item.</param>
        /// <param name="parameters">A collection of parameters to be passed to the accessor function as arguments.</param>
        public Initializer(string name, Func<object[], object> accessor, IEnumerable<Parameter> parameters)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(accessor);
            ArgumentNullException.ThrowIfNull(parameters);

            this.Name = name;
            this.Accessor = accessor;
            this.Parameters = parameters.ToList().AsReadOnly();
        }

        /// <inheritdoc/>
        public Func<object[], object> Accessor { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IReadOnlyList<Parameter> Parameters { get; }

        /// <inheritdoc/>
        public override string ToString() => this.Name;
    }
}
