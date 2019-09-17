// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    public partial class GameDisplayForm : Form
    {
        private readonly IReadOnlyList<ObjectGraphEditor.Display> displays;

        public GameDisplayForm(IGameInfo gameInfo)
        {
            this.InitializeComponent();
            this.GameInfo = gameInfo;
            this.GameInfo.Move += this.GameInfo_Move;
            this.displays = new List<ObjectGraphEditor.Display>
            {
                new PlayerTokenDisplay(this.GameInfo),
            };
            this.RefreshDisplay();
        }

        public IGameInfo GameInfo { get; }

        private void GameInfo_Move(object sender, EventArgs e)
        {
            this.RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            var control = ObjectGraphEditor.MakeDisplay(null, this.GameInfo.Game.Name, this.GameInfo.Game.GameStateType, this.GameInfo.GameStates.Last(), this.displays);
            this.splitContainer.Panel1.Controls.Clear(); // TODO: Dispose controls.
            this.splitContainer.Panel1.Controls.Add(control);
        }

        private class PlayerTokenDisplay : ObjectGraphEditor.Display
        {
            private IGameInfo gameInfo;

            public PlayerTokenDisplay(IGameInfo gameInfo)
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

            public override bool CanDisplay(string path, string name, Type type, object value) =>
                value is PlayerToken playerToken && this.gameInfo.PlayerTokens.Contains(playerToken);

            public override Control Create(string path, string name, Type type, object value, IReadOnlyList<ObjectGraphEditor.Display> overrideDisplays)
            {
                var playerToken = (PlayerToken)value;
                var label = ObjectGraphEditor.MakeLabel(this.gameInfo.PlayerNames[this.gameInfo.PlayerTokens.IndexOf(playerToken)]);
                label.ForeColor = GetPlayerColor(playerToken);
                return label;
            }
        }
    }
}
