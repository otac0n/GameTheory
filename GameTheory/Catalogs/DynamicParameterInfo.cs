// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A convenience class implementing <see cref="ParameterInfo"/>.
    /// </summary>
    public class DynamicParameterInfo : ParameterInfo
    {
        private readonly IList<CustomAttributeData> customAttributes;
        private readonly bool hasDefaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicParameterInfo"/> class.
        /// </summary>
        /// <param name="name">The name of this parameter.</param>
        /// <param name="parameterType">The Type of the parameter.</param>
        /// <param name="position">The zero-based position of the parameter in the parameter list.</param>
        /// <param name="hasDefaultValue">A value that indicates whether this parameter has a default value.</param>
        /// <param name="defaultValue">A value indicating the default value if the parameter has a default value.</param>
        /// <param name="member">A value indicating the member in which the parameter is implemented.</param>
        /// <param name="customAttribtues">A collection that contains this parameter's custom attributes.</param>
        public DynamicParameterInfo(
            string name,
            Type parameterType,
            int position,
            bool hasDefaultValue,
            object defaultValue,
            MemberInfo member,
            CustomAttributeData[] customAttribtues)
        {
            this.NameImpl = name;
            this.ClassImpl = parameterType;
            this.PositionImpl = position;
            this.AttrsImpl = hasDefaultValue ? ParameterAttributes.HasDefault : ParameterAttributes.None;
            this.MemberImpl = member;
            this.customAttributes = customAttribtues == null
                ? Array.Empty<CustomAttributeData>()
                : (IList<CustomAttributeData>)customAttribtues.ToList().AsReadOnly();

            this.hasDefaultValue = hasDefaultValue;
            if (hasDefaultValue)
            {
                this.DefaultValueImpl = defaultValue;
            }
            else if (defaultValue is object)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultValue));
            }
        }

        /// <inheritdoc/>
        public override object DefaultValue => this.hasDefaultValue ? this.DefaultValueImpl : throw new InvalidOperationException();

        /// <inheritdoc/>
        public override bool HasDefaultValue => this.hasDefaultValue;

        /// <inheritdoc/>
        public override object RawDefaultValue => this.DefaultValue;

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(bool inherit) =>
            this.GetCustomAttributesData().Select(Create).ToArray();

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) =>
            this.GetCustomAttributesData().Where(Filter(attributeType)).Select(Create).ToArray();

        /// <inheritdoc/>
        public override IList<CustomAttributeData> GetCustomAttributesData() => this.customAttributes;

        /// <inheritdoc/>
        public override bool IsDefined(Type attributeType, bool inherit) =>
            this.GetCustomAttributesData().Any(Filter(attributeType));

        private static Attribute Create(CustomAttributeData attribute) =>
            (Attribute)attribute.Constructor.Invoke(attribute.ConstructorArguments.Select(c => c.Value).ToArray());

        private static Func<CustomAttributeData, bool> Filter(Type attributeType) =>
            a => attributeType.IsAssignableFrom(a.AttributeType);
    }
}
