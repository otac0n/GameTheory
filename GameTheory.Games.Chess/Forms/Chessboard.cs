// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Forms
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;

    public class Chessboard : Control
    {
        private readonly NotationSystem NotationSystem = new NotationSystems.FigurineAlgebraicNotation();

        private GameState gameState;

        public Chessboard(GameState gameState, PlayerToken playerToken = null)
        {
            this.gameState = gameState;
            this.PlayerToken = playerToken;
            this.DoubleBuffered = true;
            this.Width = 50 * gameState.Variant.Width;
            this.Height = 50 * gameState.Variant.Height;
        }

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

        public Color WhiteColor { get; set; } = Color.White;

        public Color BlackColor { get; set; } = Color.Black;

        public Color DarkColor { get; set; } = Color.FromArgb(0xb5, 0x88, 0x63);

        public Color LightColor { get; set; } = Color.FromArgb(0xf0, 0xd9, 0xb5);

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.High;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            e.Graphics.FillRectangle(new SolidBrush(this.LightColor), this.ClientRectangle);

            var w = this.GameState.Variant.Width;
            var h = this.GameState.Variant.Height;

            float lerpX(float x) => x * this.ClientRectangle.Width / w + this.ClientRectangle.Left;
            float lerpY(float y) => y * this.ClientRectangle.Height / h + this.ClientRectangle.Top;

            var flip = this.PlayerToken == this.GameState.Players[1];
            for (var y = 0; y < h; y++)
            {
                float fromY, toY;
                if (flip)
                {
                    fromY = lerpY(y);
                    toY = lerpY(y + 1);
                }
                else
                {
                    fromY = lerpY(h - (y + 1));
                    toY = lerpY(h - y);
                }

                for (var x = 0; x < w; x++)
                {
                    float fromX, toX;
                    if (flip)
                    {
                        fromX = lerpX(w - (x + 1));
                        toX = lerpX(w - x);
                    }
                    else
                    {
                        fromX = lerpX(x);
                        toX = lerpX(x + 1);
                    }

                    var rect = new RectangleF(fromX, fromY, toX - fromX, toY - fromY);

                    if ((x + y) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(this.DarkColor), rect);
                    }

                    var piece = this.GameState.GetPieceAt(x, y);
                    if (piece == Pieces.None)
                    {
                        continue;
                    }

                    Color color, outline;
                    if ((piece & Pieces.White) == Pieces.White)
                    {
                        piece = (piece ^ Pieces.White) | Pieces.Black;
                        color = this.WhiteColor;
                        outline = this.BlackColor;
                    }
                    else if ((piece & Pieces.Black) == Pieces.Black)
                    {
                        color = this.BlackColor;
                        outline = this.WhiteColor;
                    }
                    else
                    {
                        color = Color.LightGray;
                        outline = Color.DarkGray;
                    }

                    var text = this.NotationSystem.Format(piece);
                    using (var path = new GraphicsPath())
                    using (var pen = new Pen(outline, 2) { LineJoin = LineJoin.Round })
                    using (var brush = new SolidBrush(color))
                    {
                        path.AddString(text, this.Font.FontFamily, (int)this.Font.Style, rect.Height * 0.75f, rect, StringFormat.GenericTypographic);
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
}
