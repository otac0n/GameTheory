// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public abstract class Display
    {
        public abstract bool CanDisplay(string path, string name, Type type, object value);

        public abstract Control Create(string path, string name, Type type, object value, IReadOnlyList<Display> overrideDisplays);
    }
}
