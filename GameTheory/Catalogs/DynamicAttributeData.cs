// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A convenience class implementing <see cref="CustomAttributeData"/>.
    /// </summary>
    public class DynamicAttributeData : CustomAttributeData
    {
        private readonly ConstructorInfo constructor;
        private readonly CustomAttributeTypedArgument[] constructorArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicAttributeData"/> class.
        /// </summary>
        /// <param name="constructor">The attribute constructor.</param>
        /// <param name="constructorArguments">The arguments to the attribut constructor.</param>
        public DynamicAttributeData(ConstructorInfo constructor, object[] constructorArguments)
        {
            this.constructor = constructor;
            this.constructorArguments = constructor.GetParameters().Zip(constructorArguments, (p, a) => new CustomAttributeTypedArgument(p.ParameterType, a)).ToArray();
        }

        /// <inheritdoc/>
        public override ConstructorInfo Constructor => this.constructor;

        /// <inheritdoc/>
        public override IList<CustomAttributeTypedArgument> ConstructorArguments => this.constructorArguments;
    }
}
