// -----------------------------------------------------------------------
// <copyright file="TileColorToBrushConverter.cs" company="(none)">
//   Copyright © 2015 Katie Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System;
    using Windows.UI;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;

    /// <summary>
    /// Converts an <see cref="TileColor"/> to a <see cref="Brush"/>.
    /// </summary>
    public class TileColorToBrushConverter : IValueConverter
    {
        private static readonly Brush BlueBrush = new SolidColorBrush(Colors.Blue);
        private static readonly Brush RedBrush = new SolidColorBrush(Colors.Red);

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Brush) || !(value is TileColor))
            {
                throw new NotSupportedException();
            }

            var color = (TileColor)value;
            return color == TileColor.Blue ? BlueBrush :
                   color == TileColor.Red ? RedBrush :
                   null;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(TileColor) || !(value is SolidColorBrush))
            {
                throw new NotSupportedException();
            }

            var color = ((SolidColorBrush)value).Color;
            return color == Colors.Blue ? TileColor.Blue :
                   color == Colors.Red ? TileColor.Red :
                   (TileColor?)null;
        }
    }
}
