// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.SevenDragons.Forms
{
    using System;
    using System.Collections.Immutable;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using GameTheory.Games.SevenDragons.Cards;

    public class TableControl : Control
    {
        public TableControl(ImmutableDictionary<GameTheory.Point, DragonCard> cards)
        {
            this.Cards = cards;
            var extents = this.Cards.Keys.Aggregate(
                new { MinX = int.MaxValue, MaxX = int.MinValue, MinY = int.MaxValue, MaxY = int.MinValue },
                (value, key) => new { MinX = Math.Min(value.MinX, key.X), MaxX = Math.Max(value.MaxX, key.X), MinY = Math.Min(value.MinY, key.Y), MaxY = Math.Max(value.MaxY, key.Y) });
            this.Width = (extents.MaxX - extents.MinX + 1) * (50 + 5) - 5;
            this.Height = (extents.MaxY - extents.MinY + 1) * (100 + 5) - 5;
        }

        public ImmutableDictionary<GameTheory.Point, DragonCard> Cards { get; }

        protected override void OnPaint(PaintEventArgs e)
        {
            var extents = this.Cards.Keys.Aggregate(
                new { MinX = int.MaxValue, MaxX = int.MinValue, MinY = int.MaxValue, MaxY = int.MinValue },
                (value, key) => new { MinX = Math.Min(value.MinX, key.X), MaxX = Math.Max(value.MaxX, key.X), MinY = Math.Min(value.MinY, key.Y), MaxY = Math.Max(value.MaxY, key.Y) });

            var nw = (extents.MaxX - extents.MinX + 1);
            var nh = (extents.MaxY - extents.MinY + 1);
            var w = Math.Min(
                this.ClientRectangle.Width / (nw + (nw - 1) * 0.1f),
                this.ClientRectangle.Height / (nh + (nh - 1) * 0.05f));
            var h = 2 * w;

            var cellSpacing = w * 0.1f;

            for (var y = 0; y < nh; y++)
            {
                for (var x = 0; x < nw; x++)
                {
                    if (this.Cards.TryGetValue(new GameTheory.Point(extents.MinX + x, extents.MinY + y), out var card))
                    {
                        var rect = new RectangleF(x * (w + cellSpacing), y * (h + cellSpacing), w, h);
                        CardControl.RenderCard(e.Graphics, card, rect, this.Font);
                    }
                }
            }
        }
    }
}
