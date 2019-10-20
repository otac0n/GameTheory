// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Draughts.Forms
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

    public class Checkerboard : Control
    {
        private GameState gameState;

        public Checkerboard(GameState gameState, PlayerToken playerToken = null)
        {
            this.gameState = gameState;
            this.PlayerToken = playerToken;
            this.DoubleBuffered = true;
            this.Width = 50 * gameState.Variant.Width;
            this.Height = 50 * gameState.Variant.Height;
            this.Font = new Font(this.Font.FontFamily, 50 * 0.75f, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        public Color BlackColor { get; set; } = Color.Black;

        public Color BlackOutlineColor { get; set; } = Color.White;

        public Color DarkColor { get; set; } = Color.DarkGray;

        public GameState GameState
        {
            get => this.gameState;

            set
            {
                this.gameState = value;
                this.Invalidate();
            }
        }

        public Color LightColor { get; set; } = Color.LightGray;

        public PlayerToken PlayerToken { get; }

        public Color RedColor { get; set; } = Color.Red;

        public Color RedOutlineColor { get; set; } = Color.Black;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.HighQuality(() =>
            {
                using (var lightBrush = new SolidBrush(this.LightColor))
                using (var darkBrush = new SolidBrush(this.DarkColor))
                {
                    e.Graphics.FillRectangle(lightBrush, this.ClientRectangle);

                    var w = this.GameState.Variant.Width;
                    var h = this.GameState.Variant.Height;

                    float lerpX(float x) => x * this.ClientRectangle.Width / w + this.ClientRectangle.Left;
                    float lerpY(float y) => y * this.ClientRectangle.Height / h + this.ClientRectangle.Top;

                    using (var numberFont = new Font(this.Font.FontFamily, 0.25f * this.ClientRectangle.Width / w, this.Font.Style, GraphicsUnit.Pixel))
                    {
                        var flip = this.PlayerToken == this.GameState.Players[1];
                        for (var y = 0; y < h; y++)
                        {
                            float fromY, toY;
                            if (flip)
                            {
                                fromY = lerpY(h - (y + 1));
                                toY = lerpY(h - y);
                            }
                            else
                            {
                                fromY = lerpY(y);
                                toY = lerpY(y + 1);
                            }

                            for (var x = 0; x < w; x++)
                            {
                                float fromX, toX;
                                if (flip)
                                {
                                    fromX = lerpX(x);
                                    toX = lerpX(x + 1);
                                }
                                else
                                {
                                    fromX = lerpX(w - (x + 1));
                                    toX = lerpX(w - x);
                                }

                                var rect = new RectangleF(fromX, fromY, toX - fromX, toY - fromY);

                                if ((x + y) % 2 == 0)
                                {
                                    e.Graphics.FillRectangle(darkBrush, rect);
                                }

                                var index = this.GameState.Variant.GetIndexOf(x, y);
                                if (index > -1)
                                {
                                    using (var format = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near })
                                    {
                                        e.Graphics.DrawString((index + 1).ToString(), numberFont, darkBrush, RectangleF.Inflate(rect, -2, -2), format);
                                    }
                                }

                                var piece = this.GameState.GetPieceAt(x, y);
                                if (piece == Pieces.None)
                                {
                                    continue;
                                }

                                Color color, outline;
                                if ((piece & Pieces.White) == Pieces.White)
                                {
                                    color = this.RedColor;
                                    outline = this.RedOutlineColor;
                                }
                                else if ((piece & Pieces.Black) == Pieces.Black)
                                {
                                    color = this.BlackColor;
                                    outline = this.BlackOutlineColor;
                                }
                                else
                                {
                                    color = Color.LightGray;
                                    outline = Color.DarkGray;
                                }

                                var text = (piece & Pieces.Crowned) == Pieces.Crowned ? "⛃" : "⛂";
                                using (var path = new GraphicsPath())
                                using (var pen = new Pen(outline, 2) { LineJoin = LineJoin.Round })
                                using (var brush = new SolidBrush(color))
                                {
                                    path.AddString(text, this.Font, e.Graphics, rect, StringFormat.GenericTypographic);
                                    var bounds = path.GetBounds();
                                    var matrix = new Matrix();
                                    matrix.Translate((rect.Width - bounds.Width) / 2 - bounds.Left + rect.Left, (rect.Height - bounds.Height) / 2 - bounds.Top + rect.Top);
                                    path.Transform(matrix);
                                    e.Graphics.DrawPath(pen, path);
                                    e.Graphics.FillPath(brush, path);
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}
