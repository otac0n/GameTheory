// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Describes an <see cref="IInitializer"/> parameter.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="parameterInfo">The <see cref="ParameterInfo"/> to use as the soruce for this parameter.</param>
        public Parameter(ParameterInfo parameterInfo)
        {
            this.ParameterType = parameterInfo.ParameterType;

            var description = parameterInfo.GetCustomAttribute<DescriptionAttribute>(inherit: true);
            var displayName = parameterInfo.GetCustomAttribute<DisplayNameAttribute>(inherit: true);
            var parenthesizePropertyName = parameterInfo.GetCustomAttribute<ParenthesizePropertyNameAttribute>(inherit: true);

            var name = displayName?.DisplayName ?? parameterInfo.Name;
            if (parenthesizePropertyName?.NeedParenthesis ?? false)
            {
                name = string.Format(SharedResources.ParameterNameParenthesis, name);
            }

            this.Name = name;
            this.Description = description?.Description;

            if (parameterInfo.HasDefaultValue)
            {
                this.Default = new Maybe<object>(parameterInfo.RawDefaultValue);
            }

            var validations = parameterInfo.GetCustomAttributes<ValidationAttribute>(inherit: true);
            this.Validations = validations.ToList().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterType">The type of the parameter.</param>
        /// <param name="default">The optional default value.</param>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="validations">A collection of validation attributes.</param>
        public Parameter(string name, Type parameterType, Maybe<object> @default = default, string description = null, IEnumerable<ValidationAttribute> validations = null)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
            this.Default = @default;
            this.Description = description;
            this.Validations = (validations ?? Array.Empty<ValidationAttribute>()).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the optional default value.
        /// </summary>
        public Maybe<object> Default { get; }

        /// <summary>
        /// Gets the description of the parameter.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets a collection of validation attributes.
        /// </summary>
        public IReadOnlyList<ValidationAttribute> Validations { get; }
    }
}
