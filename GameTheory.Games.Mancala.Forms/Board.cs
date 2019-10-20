// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Mancala.Forms
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

    public class Board : Control
    {
        private GameState gameState;

        public Board(GameState gameState, PlayerToken playerToken = null)
        {
            this.gameState = gameState;
            this.PlayerToken = playerToken;
            this.DoubleBuffered = true;
            this.Width = (gameState.BinsPerSide + 2) * (50 + 5) - 5;
            this.Height = 2 * (50 + 5) - 5;
        }

        public Color BinColor { get; set; } = Color.LightGray;

        public GameState GameState
        {
            get => this.gameState;

            set
            {
                this.gameState = value;
                this.Invalidate();
            }
        }

        public PlayerToken PlayerToken { get; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.HighQuality(() =>
            {
                e.Graphics.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);

                var nw = this.GameState.BinsPerSide + 2;
                var nh = 2;
                var w = Math.Min(
                    this.ClientRectangle.Width / (nw + (nw - 1) * 0.1f),
                    this.ClientRectangle.Height / (nh + (nh - 1) * 0.1f));

                var cellSpacing = w * 0.1f;

                using (var binBrush = new SolidBrush(this.BinColor))
                using (var textBrush = new SolidBrush(this.ForeColor))
                using (var stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    for (var i = 0; i < (this.GameState.BinsPerSide + 1) * 2; i++)
                    {
                        var isMancala = i == this.GameState.BinsPerSide || i == this.GameState.BinsPerSide + (this.GameState.BinsPerSide + 1);
                        var y = isMancala ? 0 : 1 - i / (this.GameState.BinsPerSide + 1);
                        var x = i <= this.GameState.BinsPerSide ? i + 1 : (this.GameState.BinsPerSide + 1) * 2 - i - 1;

                        var rect = new RectangleF(x * (w + cellSpacing), y * (w + cellSpacing), w, isMancala ? 2 * w + cellSpacing : w);
                        e.Graphics.FillRectangle(binBrush, rect);
                        e.Graphics.DrawString(this.GameState[i].ToString(), this.Font, textBrush, rect, stringFormat);
                    }
                }
            });
        }
    }
}
