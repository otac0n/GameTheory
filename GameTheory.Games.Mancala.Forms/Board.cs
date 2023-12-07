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
            var g = e.Graphics;
            g.HighQuality(() =>
            {
                g.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);

                var nw = this.GameState.BinsPerSide + 2;
                var nh = 2;
                var w = Math.Min(
                    this.ClientRectangle.Width / (nw + (nw - 1) * 0.1f),
                    this.ClientRectangle.Height / (nh + (nh - 1) * 0.1f));

                var cellSpacing = w * 0.1f;

                const int ScaleCount = 10;
                var beanRadius = Math.Sqrt(Math.Pow(w, 2) / (ScaleCount * 2));
                var beanRect = new RectangleF(0, 0, (float)beanRadius, (float)(beanRadius * 0.8));

                using (var beanPen = new Pen(this.BinColor))
                using (var beanBrush = new SolidBrush(this.BackColor))
                using (var binBrush = new SolidBrush(this.BinColor))
                using (var textBrush = new SolidBrush(this.ForeColor))
                using (var stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    for (var i = 0; i < (this.GameState.BinsPerSide + 1) * 2; i++)
                    {
                        var isMancala = i == this.GameState.BinsPerSide || i == this.GameState.BinsPerSide + (this.GameState.BinsPerSide + 1);
                        var h = isMancala ? 2 * w + cellSpacing : w;
                        var y = isMancala ? 0 : 1 - i / (this.GameState.BinsPerSide + 1);
                        var x = i <= this.GameState.BinsPerSide ? i + 1 : (this.GameState.BinsPerSide + 1) * 2 - i - 1;
                        var count = this.GameState[i];

                        var rect = new RectangleF(x * (w + cellSpacing), y * (w + cellSpacing), w, h);
                        g.FillRectangle(binBrush, rect);

                        if (count > 0)
                        {
                            var random = new Random(i);
                            for (var b = 0; b < count; b++)
                            {
                                var bx = beanRadius + Math.Clamp(random.NextDoubleNormal(0.5, 0.2), 0, 1) * (w - 2 * beanRadius);
                                var by = beanRadius + Math.Clamp(random.NextDoubleNormal(0.5, 0.2), 0, 1) * (h - 2 * beanRadius);
                                g.TranslateTransform((float)(rect.X + bx), (float)(rect.Y + by));

                                var angleDeg = random.NextDouble() * 360;
                                g.RotateTransform((float)angleDeg);

                                g.FillEllipse(beanBrush, beanRect);
                                g.DrawEllipse(beanPen, beanRect);
                                g.ResetTransform();
                            }
                        }

                        g.DrawString(count.ToString(), this.Font, textBrush, rect, stringFormat);
                    }
                }
            });
        }
    }
}
