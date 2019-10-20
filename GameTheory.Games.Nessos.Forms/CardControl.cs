// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Forms
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using GameTheory.FormsRunner.Shared;

    public class CardControl : Control
    {
        private static readonly Dictionary<Card, HatchBrush> CardBrush = new Dictionary<Card, HatchBrush>
        {
            [Card.Satyr] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Centaur] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.NemeanLion] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Minotaur] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Griffin] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Phoenix] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Pegasus] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Cerberus] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.LerneanHydra] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Medusa] = new HatchBrush(HatchStyle.Weave, Color.Firebrick, Color.Goldenrod),
            [Card.Charon] = new HatchBrush(HatchStyle.Wave, Color.DarkKhaki, Color.DarkSeaGreen),
        };

        private static readonly HatchBrush CardReverseBrush = new HatchBrush(HatchStyle.Trellis, Color.Gold, Color.Black);
        private Card card;

        public CardControl(Card card, bool showReverse)
        {
            this.card = card;
            this.ShowReverse = showReverse;
            this.BackColor = Color.Black;
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
            graphics.HighQuality(() =>
            {
                var brush = showReverse ? CardReverseBrush : CardBrush[card];
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
            });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RenderCard(e.Graphics, this.Card, this.ShowReverse, this.ClientRectangle, this.Font);
        }
    }
}
