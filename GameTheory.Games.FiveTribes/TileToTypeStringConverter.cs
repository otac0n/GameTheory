// -----------------------------------------------------------------------
// <copyright file="TileToTypeStringConverter.cs" company="(none)">
//   Copyright © 2015 Katie Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace GameTheory.Games.FiveTribes
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;

    /// <summary>
    /// Converts a <see cref="Tile"/> to a string representation.
    /// </summary>
    public class TileToTypeStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            var name = value.GetType().Name;
            return string.Concat(Enumerable.Range(0, name.Length).Select(i => name[i]).Where(c => char.IsUpper(c)));
        }

        /// <inheritdoc />
        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
