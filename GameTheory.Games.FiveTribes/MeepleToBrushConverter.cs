// -----------------------------------------------------------------------
// <copyright file="MeepleToBrushConverter.cs" company="(none)">
//   Copyright © 2015 Katie Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Collections.Generic;
    using Windows.UI;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;

    /// <summary>
    /// Converts a <see cref="Meeple"/> to a <see cref="Brush"/>.
    /// </summary>
    public class MeepleToBrushConverter : IValueConverter
    {
        private static readonly Dictionary<Meeple, Brush> BrushLookup = new Dictionary<Meeple, Brush>
        {
            { Meeple.Assassin, new SolidColorBrush(Colors.Red) },
            { Meeple.Builder, new SolidColorBrush(Colors.Blue) },
            { Meeple.Elder, new SolidColorBrush(Colors.White) },
            { Meeple.Merchant, new SolidColorBrush(Colors.Green) },
            { Meeple.Vizier, new SolidColorBrush(Colors.Yellow) },
        };

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return BrushLookup[(Meeple)value];
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
