// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.ConsoleRunner.Shared
{
    using System;

    /// <summary>
    /// Specifies the console font required.
    /// </summary>
    public class ConsoleFontAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleFontAttribute"/> class.
        /// </summary>
        /// <param name="name">The font name.</param>
        /// <param name="xSize">The x-Size value.</param>
        /// <param name="ySize">The y-Size value.</param>
        public ConsoleFontAttribute(string name, short xSize = 0, short ySize = 0)
        {
            this.Name = name;
            this.XSize = xSize;
            this.YSize = ySize;
        }

        /// <summary>
        /// Gets the font name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the x-Size.
        /// </summary>
        public short XSize { get; }

        /// <summary>
        /// Gets the y-Size.
        /// </summary>
        public short YSize { get; }
    }
}
