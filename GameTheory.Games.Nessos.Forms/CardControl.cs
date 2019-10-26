// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Nessos.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
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
        private bool showAsUncertain;
        private bool showReverse;

        public CardControl(Card card, bool showReverse, bool showAsUncertain)
        {
            this.card = card;
            this.showReverse = showReverse;
            this.showAsUncertain = showAsUncertain;
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

        public bool ShowAsUncertain
        {
            get => this.showAsUncertain;

            set
            {
                if (this.showAsUncertain != value)
                {
                    this.showAsUncertain = value;
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

        public static void RenderCard(Graphics graphics, Card card, bool showReverse, bool showAsUncertain, Rectangle rectangle, Font font)
        {
            RenderCard(graphics, card, showReverse, showAsUncertain, new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), font);
        }

        public static void RenderCard(Graphics graphics, Card card, bool showReverse, bool showAsUncertain, RectangleF rectangle, Font font)
        {
            if (showAsUncertain)
            {
                var width = (int)Math.Ceiling(rectangle.Size.Width);
                var height = (int)Math.Ceiling(rectangle.Size.Height);
                var sourceRectangle = new RectangleF(PointF.Empty, new SizeF(width, height));
                var bitmap = new Bitmap(width, height, graphics);
                RenderCard(Graphics.FromImage(bitmap), card, showReverse, false, sourceRectangle, font);
                graphics.DrawImageTransparent(bitmap, rectangle, 0.5f);
                return;
            }

            graphics.HighQuality(() =>
            {
                if (showReverse)
                {
                    graphics.FillRectangle(CardReverseBrush, rectangle);
                    graphics.DrawRectangle(new Pen(Color.Black), rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                }
                else
                {
                    graphics.FillRectangle(CardBrush[card], rectangle);
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
            RenderCard(e.Graphics, this.Card, this.ShowReverse, this.ShowAsUncertain, this.ClientRectangle, this.Font);
        }
    }
}
