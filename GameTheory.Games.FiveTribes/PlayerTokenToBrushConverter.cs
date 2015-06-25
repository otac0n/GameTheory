// -----------------------------------------------------------------------
// <copyright file="PlayerTokenToBrushConverter.cs" company="(none)">
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
    /// Converts a <see cref="PlayerToken"/> and to <see cref="Brush"/>.
    /// </summary>
    public class PlayerTokenToBrushConverter : IValueConverter
    {
        private static readonly Brush[] Brushes =
        {
            new SolidColorBrush(Colors.Pink),
            new SolidColorBrush(Colors.Teal),
            new SolidColorBrush(Colors.Orange),
            new SolidColorBrush(Colors.DarkGray),
        };

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null ? null : Brushes[((GameState)parameter).Players.IndexOf((PlayerToken)value)];
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
