// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A catalog of available players.
    /// </summary>
    public interface IPlayerCatalog
    {
        /// <summary>
        /// Gets the list of players who are capable of playing the specified move type.
        /// </summary>
        /// <param name="gameStateType">The type of game states the players must support.</param>
        /// <param name="moveType">The type of moves to be played.</param>
        /// <returns>A collection of supported players.</returns>
        IReadOnlyList<ICatalogPlayer> FindPlayers(Type gameStateType, Type moveType);
    }
}
