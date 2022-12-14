// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.LoveLetter.Forms
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

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
        private bool showReverse;

        public CardControl(Card card, bool showReverse)
        {
            this.card = card;
            this.showReverse = showReverse;
            this.DoubleBuffered = true;
            this.Width = 50;
            this.Height = 100;
            this.Font = new Font(this.Font.FontFamily, 8.0f);
        }

        public Card Card
        {
            get => this.card;

            set
            {
                if (this.card != value)
                {
                    this.card = value;
                    this.Invalidate();
                }
            }
        }

        public bool ShowReverse
        {
            get => this.showReverse;

            set
            {
                if (this.showReverse != value)
                {
                    this.showReverse = value;
                    this.Invalidate();
                }
            }
        }

        public static void RenderCard(Graphics graphics, Card card, bool showReverse, Rectangle rectangle, Font font)
        {
            RenderCard(graphics, card, showReverse, new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), font);
        }

        public static void RenderCard(Graphics graphics, Card card, bool showReverse, RectangleF rectangle, Font font)
        {
            graphics.HighQuality(() =>
            {
                if (showReverse || card == Card.None)
                {
                    var brush = CardBrush[Card.None];
                    graphics.FillRectangle(brush, rectangle);
                    graphics.DrawRectangle(new Pen(Color.Black), rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                }
                else
                {
                    var brush = CardBrush[card];
                    graphics.FillRectangle(brush, rectangle);
                    graphics.DrawRectangle(new Pen(Color.Black), rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);

                    using (var textPath = new GraphicsPath())
                    using (var textPen = new Pen(Color.White, 3) { LineJoin = LineJoin.Round })
                    using (var cardValueFormat = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, })
                    using (var cardNameFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        var number = ((int)card).ToString();
                        var name = Resources.ResourceManager.GetEnumString(card);
                        graphics.OutlineString(number, new Font(font.FontFamily, font.Size * 2, font.Style, GraphicsUnit.Point), Brushes.Black, textPen, rectangle, cardValueFormat);
                        graphics.OutlineString(name, font, Brushes.Black, textPen, rectangle, cardNameFormat);
                    }
                }
            });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RenderCard(e.Graphics, this.Card, this.ShowReverse, this.ClientRectangle, this.Font);
        }
    }
}
