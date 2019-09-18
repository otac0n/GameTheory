// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class NullDisplay : Display
    {
        private NullDisplay()
        {
        }

        public static NullDisplay Instance { get; } = new NullDisplay();

        public override bool CanDisplay(string path, string name, Type type, object value) => value is null;

        public override Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays) => ObjectGraphEditor.MakeLabel("(null)");
    }
}
