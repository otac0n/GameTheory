// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A convenience class for dealing with types implementing <see cref="IGameState{TMove}"/>.
    /// </summary>
    public sealed class CatalogGame : ICatalogGame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogGame"/> class.
        /// </summary>
        /// <param name="gameStateType">The type used as a game state.</param>
        /// <param name="moveType">The type type used for moves.</param>
        /// <param name="initializers">The initializers used to create instances of the game state.</param>
        /// <param name="name">The name of the game.</param>
        public CatalogGame(Type gameStateType, Type moveType, IEnumerable<Initializer> initializers, string name = null)
        {
            ArgumentNullException.ThrowIfNull(gameStateType);
            ArgumentNullException.ThrowIfNull(moveType);
            ArgumentNullException.ThrowIfNull(initializers);

            this.GameStateType = gameStateType;
            this.MoveType = moveType;
            this.Initializers = initializers.ToList().AsReadOnly();
            this.Name = string.IsNullOrEmpty(name)
                ? GetGameName(this.GameStateType)
                : name;
        }

        /// <inheritdoc/>
        public Type GameStateType { get; }

        /// <inheritdoc/>
        public IReadOnlyList<Initializer> Initializers { get; }

        /// <inheritdoc/>
        public Type MoveType { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString() => this.Name;

        private static string GetGameName(Type gameType)
        {
            var @namespace = gameType.Namespace;

            return FormatUtilities.GetGameStateResource(gameType, "Name") ?? @namespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        }
    }
}
