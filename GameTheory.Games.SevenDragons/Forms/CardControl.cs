// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using GameTheory.Games.SevenDragons.Cards;

    public class CardControl : Control
    {
        private static readonly Color CardBackgroundColor = Color.Black;
        private static readonly Dictionary<SevenDragons.Color, Func<RectangleF, Brush>> ColorMap = new Dictionary<SevenDragons.Color, Func<RectangleF, Brush>>
        {
            [SevenDragons.Color.Black] = _ => Brushes.DimGray,
            [SevenDragons.Color.Blue] = _ => Brushes.Blue,
            [SevenDragons.Color.Gold] = _ => Brushes.Gold,
            [SevenDragons.Color.Green] = _ => Brushes.Green,
            [SevenDragons.Color.Rainbow] = rect =>
            {
                var colors = new Color[]
                {
                    Color.FromArgb(255, 0, 0),
                    Color.FromArgb(255, 0, 0),
                    Color.FromArgb(255, 128, 0),
                    Color.FromArgb(255, 255, 0),
                    Color.FromArgb(0, 255, 0),
                    Color.FromArgb(0, 255, 128),
                    Color.FromArgb(0, 255, 255),
                    Color.FromArgb(0, 128, 255),
                    Color.FromArgb(0, 0, 255),
                    Color.FromArgb(0, 0, 255),
                };

                var colorPositions = new float[colors.Length];
                var lastIndex = colors.Length - 1;
                for (var i = 1; i < colors.Length; i++)
                {
                    colorPositions[i] = (float)i / lastIndex;
                }

                return new LinearGradientBrush(rect, colors[0], colors[lastIndex], 1.0F)
                {
                    InterpolationColors = new ColorBlend
                    {
                        Colors = colors,
                        Positions = colorPositions,
                    },
                };
            },
            [SevenDragons.Color.Red] = _ => Brushes.Red,
            [SevenDragons.Color.Silver] = _ => Brushes.Gainsboro,
        };

        public CardControl(Card card)
        {
            this.Card = card;
            this.BackColor = Color.Black;
            this.Width = 50;
            this.Height = 100;
        }

        public Card Card { get; }

        public static void RenderCard(Graphics graphics, Card card, Rectangle rectangle, Font font)
        {
            RenderCard(graphics, card, new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), font);
        }

        public static void RenderCard(Graphics graphics, Card card, RectangleF rectangle, Font font)
        {
            graphics.InterpolationMode = InterpolationMode.High;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            graphics.FillRectangle(new SolidBrush(CardBackgroundColor), rectangle);

            var cellSpacing = (int)Math.Round(rectangle.Width * 0.075);
            var rect = new RectangleF(
                rectangle.Left + cellSpacing,
                rectangle.Top + cellSpacing,
                rectangle.Width - 2 * cellSpacing,
                rectangle.Height - 2 * cellSpacing);

            switch (card)
            {
                case ActionCard actionCard:
                    graphics.FillRectangle(ColorMap[actionCard.Color](rect), rect);
                    var text = string.Empty;
                    var symbol = string.Empty;
                    switch (actionCard)
                    {
                        case ZapACardCard zapACardCard:
                            text = Resources.ZapACard;
                            symbol = "⚡";
                            break;

                        case RotateGoalsCard rotateGoalsCard:
                            text = Resources.RotateGoals;
                            symbol = "↻";
                            break;

                        case MoveACardCard moveACardCard:
                            text = Resources.MoveACard;
                            symbol = "➶";
                            break;

                        case TradeHandsCard tradeHandsCard:
                            text = Resources.TradeHands;
                            symbol = "⇆";
                            break;

                        case TradeGoalsCard tradeGoalsCard:
                            text = Resources.TradeGoals;
                            symbol = "⇆";
                            break;
                    }

                    using (var symbolPath = new GraphicsPath())
                    using (var textPath = new GraphicsPath())
                    using (var symbolPen = new Pen(Color.White, 3) { LineJoin = LineJoin.Round })
                    using (var textPen = new Pen(Color.White, 2) { LineJoin = LineJoin.Round })
                    using (var stringFormat = new StringFormat() { Alignment = StringAlignment.Center })
                    {
                        symbolPath.AddString(symbol, font.FontFamily, (int)font.Style, rect.Height * 0.4f, rect, StringFormat.GenericTypographic);
                        var bounds = symbolPath.GetBounds();
                        var matrix = new Matrix();
                        matrix.Translate((rect.Width - bounds.Width) / 2 - bounds.Left + rect.Left, (rect.Height - bounds.Height) / 2 - bounds.Top + rect.Top);
                        symbolPath.Transform(matrix);
                        bounds = symbolPath.GetBounds();
                        textPath.AddString(text, font.FontFamily, (int)font.Style, font.Size, new RectangleF(rect.Left, bounds.Bottom + font.Size, rect.Width, rect.Bottom - (bounds.Bottom + font.Size)), stringFormat);
                        graphics.DrawPath(symbolPen, symbolPath);
                        graphics.DrawPath(textPen, textPath);
                        graphics.FillPath(Brushes.Black, symbolPath);
                        graphics.FillPath(Brushes.Black, textPath);
                    }
                    break;

                case DragonCard dragonCard:
                    var w = (rect.Width - (DragonCard.Grid.Width - 1) * cellSpacing) / DragonCard.Grid.Width;
                    var h = (rect.Height - (DragonCard.Grid.Height - 1) * cellSpacing) / DragonCard.Grid.Height;

                    for (var v = 0; v < DragonCard.Grid.Height; v++)
                    {
                        for (var u = 0; u < DragonCard.Grid.Width; u++)
                        {
                            var color = dragonCard.Colors[DragonCard.Grid.IndexOf(u, v)];
                            if (v > 0 && dragonCard.Colors[DragonCard.Grid.IndexOf(u, v - 1)] == color ||
                                u > 0 && dragonCard.Colors[DragonCard.Grid.IndexOf(u - 1, v)] == color)
                            {
                                continue;
                            }

                            int u2;
                            for (u2 = u + 1; u2 < DragonCard.Grid.Width; u2++)
                            {
                                if (dragonCard.Colors[DragonCard.Grid.IndexOf(u2, v)] != color)
                                {
                                    break;
                                }
                            }

                            u2--;

                            int v2;
                            for (v2 = v + 1; v2 < DragonCard.Grid.Height; v2++)
                            {
                                if (dragonCard.Colors[DragonCard.Grid.IndexOf(u2, v2)] != color)
                                {
                                    break;
                                }
                            }

                            v2--;

                            var spot = new RectangleF(rect.Left + u * (w + cellSpacing), rect.Top + v * (h + cellSpacing), w + (u2 - u) * (w + cellSpacing), h + (v2 - v) * (h + cellSpacing));
                            graphics.FillRectangle(ColorMap[color](spot), spot);
                        }
                    }

                    break;

                default:
                    return;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RenderCard(e.Graphics, this.Card, this.ClientRectangle, this.Font);
        }
    }
}
