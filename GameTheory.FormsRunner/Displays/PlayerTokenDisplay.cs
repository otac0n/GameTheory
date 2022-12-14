// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Displays
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using GameTheory.Catalogs;
    using GameTheory.FormsRunner.Shared;
    using static GameTheory.FormsRunner.Shared.Controls;

    internal class PlayerTokenDisplay : Display
    {
        private ICatalogGame gameInfo;

        public PlayerTokenDisplay(ICatalogGame gameInfo)
        {
            this.gameInfo = gameInfo;
        }

        public static Color GetPlayerColor(PlayerToken playerToken)
        {
            var bad = SystemColors.Control;
            var bytes = playerToken.Id.ToByteArray();
            var r = (int)bytes[0];
            var g = (int)bytes[1];
            var b = (int)bytes[2];

            int[] v = { r - bad.R, g - bad.G, b - bad.B };
            double[] t =
            {
                v[0] == 0 ? double.PositiveInfinity : v[0] > 0 ? (255.0 - r) / v[0] : (0.0 - r) / v[0],
                v[1] == 0 ? double.PositiveInfinity : v[1] > 0 ? (255.0 - g) / v[1] : (0.0 - g) / v[1],
                v[2] == 0 ? double.PositiveInfinity : v[2] > 0 ? (255.0 - b) / v[2] : (0.0 - b) / v[2],
            };

            var minT = Math.Min(t[0], Math.Min(t[1], t[2]));

            r = (int)Math.Round(r + v[0] * minT);
            g = (int)Math.Round(g + v[1] * minT);
            b = (int)Math.Round(b + v[2] * minT);

            return Color.FromArgb(r, g, b);
        }

        public override bool CanDisplay(Scope scope, Type type, object value) => value is PlayerToken;

        protected override Control Update(Control control, Scope scope, Type type, object value, IReadOnlyList<Display> displays)
        {
            var playerToken = (PlayerToken)value;
            var playerName = playerToken.ToString(); // TODO: Get player names back.

            if (control is Label label && label.Tag == this)
            {
                label.Text = playerName;
            }
            else
            {
                label = MakeLabel(playerName, tag: this);
            }

            label.ForeColor = GetPlayerColor(playerToken);
            return label;
        }
    }
}
