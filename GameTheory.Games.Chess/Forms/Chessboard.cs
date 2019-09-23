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

        public Chessboard(GameState gameState, PlayerToken playerToken = null)
        {
            this.GameState = gameState;
            this.PlayerToken = playerToken;
            this.Width = 50 * gameState.Variant.Width;
            this.Height = 50 * gameState.Variant.Height;
        }

        public GameState GameState { get; }

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

            for (var y = 0; y < h; y++)
            {
                var fromY = (h - (y + 1)) * this.ClientRectangle.Height / (float)h + this.ClientRectangle.Top;
                var toY = (h - y) * this.ClientRectangle.Height / (float)h + this.ClientRectangle.Top;

                for (var x = 0; x < w; x++)
                {
                    var fromX = x * this.ClientRectangle.Width / (float)w + this.ClientRectangle.Left;
                    var toX = (x + 1) * this.ClientRectangle.Width / (float)w + this.ClientRectangle.Left;
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
