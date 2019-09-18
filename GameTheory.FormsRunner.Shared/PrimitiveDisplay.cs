// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class PrimitiveDisplay : Display
    {
        private PrimitiveDisplay()
        {
        }

        public static PrimitiveDisplay Instance { get; } = new PrimitiveDisplay();

        public override bool CanDisplay(string path, string name, Type type, object value) => type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(Guid);

        public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays) => ObjectGraphEditor.MakeLabel(value.ToString());
    }
}
