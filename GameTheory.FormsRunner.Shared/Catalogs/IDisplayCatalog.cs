// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared.Catalogs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A catalog of available displays.
    /// </summary>
    public interface IDisplayCatalog
    {
        /// <summary>
        /// Gets the list of displays who are capable of displaying elements in the specified game state type.
        /// </summary>
        /// <param name="gameStateType">The type of game state to be displayed.</param>
        /// <returns>A collection of supported displays.</returns>
        IReadOnlyList<Type> FindDisplays(Type gameStateType);
    }
}
