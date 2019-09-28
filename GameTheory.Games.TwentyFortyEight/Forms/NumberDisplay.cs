// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.TwentyFortyEight.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;
    using static FormsRunner.Shared.Controls;

    public class NumberDisplay : Display
    {
        private static Color DarkColor = Color.FromArgb(0x77, 0x6e, 0x65);
        private static Color LightColor = Color.FromArgb(0xf9, 0xf6, 0xf2);
        private static readonly Color[,] DisplayColors = new Color[,]
        {
            { DarkColor, Color.FromArgb((int)(238 * 0.35 + 0xbb * (1 - 0.35)), (int)(228 * 0.35 + 0xad * (1 - 0.35)), (int)(218 * 0.35 + 0xa0 * (1 - 0.35))) },
            { DarkColor, Color.FromArgb(0xee, 0xe4, 0xda) },
            { DarkColor, Color.FromArgb(0xed, 0xe0, 0xc8) },
            { LightColor, Color.FromArgb(0xf2, 0xb1, 0x79) },
            { LightColor, Color.FromArgb(0xf5, 0x95, 0x63) },
            { LightColor, Color.FromArgb(0xf6, 0x7c, 0x5f) },
            { LightColor, Color.FromArgb(0xf6, 0x5e, 0x3b) },
            { LightColor, Color.FromArgb(0xed, 0xcf, 0x72) },
            { LightColor, Color.FromArgb(0xed, 0xcc, 0x61) },
            { LightColor, Color.FromArgb(0xed, 0xc8, 0x50) },
            { LightColor, Color.FromArgb(0xed, 0xc5, 0x3f) },
            { LightColor, Color.FromArgb(0xed, 0xc2, 0x2e) },
            { LightColor, Color.FromArgb(0x3c, 0x3a, 0x32) },
        };

        public override bool CanDisplay(string path, string name, Type type, object value) => type == typeof(byte);

        protected override Control Update(Control control, string path, string name, Type type, object value, IReadOnlyList<Display> displays)
        {
            var number = (byte)value;
            var display = number == 0 ? string.Empty : Math.Pow(2, number).ToString();

            if (control is Label label && label.Tag == this)
            {
                label.Text = display;
            }
            else
            {
                label = MakeLabel(display, tag: this);
                label.AutoSize = false;
                label.Width = 50;
                label.Height = 50;
                label.Margin = new Padding(2);
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Font = new Font(label.Font, FontStyle.Bold);
            }

            label.ForeColor = DisplayColors[number, 0];
            label.BackColor = DisplayColors[number, 1];
            return label;
        }
    }
}
