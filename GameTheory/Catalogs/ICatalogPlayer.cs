// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface representing a set of types implementing <see cref="IPlayer{TGameState, TMove}"/>.
    /// </summary>
    public interface ICatalogPlayer
    {
        /// <summary>
        /// Gets the type used for game states.
        /// </summary>
        Type GameStateType { get; }

        /// <summary>
        /// Gets a collection of initializers, representing different ways to create this player.
        /// </summary>
        IReadOnlyList<Initializer> Initializers { get; }

        /// <summary>
        /// Gets the type used for moves.
        /// </summary>
        Type MoveType { get; }

        /// <summary>
        /// Gets the name of the player.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the player.
        /// </summary>
        Type PlayerType { get; }
    }
}
