// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Forms
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;

    public class CardControl : Control
    {
        private static readonly Dictionary<Card, HatchBrush> CardBrush = new Dictionary<Card, HatchBrush>
        {
            [Card.None] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Maroon),
            [Card.Guard] = new HatchBrush(HatchStyle.DashedVertical, Color.Gold, Color.Goldenrod),
            [Card.Priest] = new HatchBrush(HatchStyle.Percent20, Color.PaleVioletRed, Color.MediumVioletRed),
            [Card.Baron] = new HatchBrush(HatchStyle.Percent20, Color.MediumSlateBlue, Color.SlateBlue),
            [Card.Handmaid] = new HatchBrush(HatchStyle.LargeConfetti, Color.Lavender, Color.LightSkyBlue),
            [Card.Prince] = new HatchBrush(HatchStyle.Divot, Color.LemonChiffon, Color.Khaki),
            [Card.King] = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Honeydew, Color.LightGreen),
            [Card.Countess] = new HatchBrush(HatchStyle.Wave, Color.Lavender, Color.LightSkyBlue),
            [Card.Princess] = new HatchBrush(HatchStyle.DottedDiamond, Color.AliceBlue, Color.Lavender),
        };

        private Card card;

        public CardControl(Card card, bool showReverse)
        {
            this.card = card;
            this.ShowReverse = showReverse;
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;
            this.Width = 50;
            this.Height = 100;
            this.Font = new Font(this.Font.FontFamily, 10.0f);
        }

        public Card Card
        {
            get => this.card;

            set
            {
                this.card = value;
                this.Invalidate();
            }
        }

        public bool ShowReverse { get; }

        public static void RenderCard(Graphics graphics, Card card, bool showReverse, Rectangle rectangle, Font font)
        {
            RenderCard(graphics, card, showReverse, new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), font);
        }

        public static void RenderCard(Graphics graphics, Card card, bool showReverse, RectangleF rectangle, Font font)
        {
            graphics.InterpolationMode = InterpolationMode.High;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            var brush = CardBrush[showReverse ? Card.None : card];
            graphics.FillRectangle(brush, rectangle);
            graphics.DrawRectangle(new Pen(Color.Black), rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);

            using (var textPath = new GraphicsPath())
            using (var textPen = new Pen(Color.White, 2) { LineJoin = LineJoin.Round })
            using (var cardValueFormat = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, })
            using (var cardNameFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                textPath.AddString(((int)card).ToString(), font.FontFamily, (int)font.Style, font.Size * 2, rectangle, cardValueFormat);
                textPath.AddString(Resources.ResourceManager.GetEnumString(card), font.FontFamily, (int)font.Style, font.Size, rectangle, cardNameFormat);
                graphics.DrawPath(textPen, textPath);
                graphics.FillPath(Brushes.Black, textPath);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RenderCard(e.Graphics, this.Card, this.ShowReverse, this.ClientRectangle, this.Font);
        }
    }
}
