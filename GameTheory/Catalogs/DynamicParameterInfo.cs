// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Reflection;

    public class DynamicParameterInfo : ParameterInfo
    {
        private readonly bool hasDefaultValue;

        public DynamicParameterInfo(
            string name,
            Type @class,
            int position,
            ParameterAttributes attributes,
            bool hasDefaultValue,
            object defaultValue,
            MemberInfo member)
        {
            this.NameImpl = name;
            this.ClassImpl = @class;
            this.PositionImpl = position;
            this.AttrsImpl = attributes;
            this.MemberImpl = member;

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

        public override bool HasDefaultValue => this.hasDefaultValue;
    }
}
