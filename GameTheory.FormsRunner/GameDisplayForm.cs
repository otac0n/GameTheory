// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner
{
    using System;
    using System.Collections.Generic;
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

            public override bool CanDisplay(string path, string name, Type type, object value) =>
                value is PlayerToken playerToken && this.gameInfo.PlayerTokens.Contains(playerToken);

            public override Control Create(string path, string name, Type type, object value, IReadOnlyList<ObjectGraphEditor.Display> overrideDisplays) =>
                ObjectGraphEditor.MakeLabel(this.gameInfo.PlayerNames[this.gameInfo.PlayerTokens.IndexOf((PlayerToken)value)]);
        }
    }
}
