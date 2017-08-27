// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a contract for rendering an object in a variety of user interfaces.
    /// </summary>
    public interface ITokenFormattable
    {
        /// <summary>
        /// Gets the sequence of strings, ITokenFormattable objects, and other objects that should be rendered to represent this object.
        /// </summary>
        IList<object> FormatTokens { get; }
    }
}
