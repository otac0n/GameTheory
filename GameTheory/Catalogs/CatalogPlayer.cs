// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A convenience class for dealing with types implementing <see cref="IPlayer{TGameState, TMove}"/>.
    /// </summary>
    public class CatalogPlayer : ICatalogPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogPlayer"/> class.
        /// </summary>
        /// <param name="playerType">The type of the player.</param>
        /// <param name="gameStateType">The type used for game states.</param>
        /// <param name="moveType">The type used for moves.</param>
        /// <param name="initializers">The initializers used to create instances of the player.</param>
        /// <param name="name">The name of the player.</param>
        public CatalogPlayer(Type playerType, Type gameStateType, Type moveType, IEnumerable<Initializer> initializers, string name = null)
        {
            this.PlayerType = playerType ?? throw new ArgumentNullException(nameof(playerType));
            this.GameStateType = gameStateType ?? throw new ArgumentNullException(nameof(gameStateType));
            this.MoveType = moveType ?? throw new ArgumentNullException(nameof(moveType));
            this.Initializers = (initializers ?? throw new ArgumentNullException(nameof(initializers))).ToList().AsReadOnly();
            this.Name = string.IsNullOrEmpty(name)
                ? GetPlayerName(this.PlayerType)
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

        /// <inheritdoc/>
        public Type PlayerType { get; }

        /// <inheritdoc />
        public override string ToString() => this.Name;

        private static string GetPlayerName(Type playerType)
        {
            var typeName = playerType.Name.Split('`').First();

            const string playerSuffix = "Player";
            if (typeName.EndsWith(playerSuffix, StringComparison.OrdinalIgnoreCase))
            {
                typeName = typeName.Substring(0, typeName.Length - playerSuffix.Length);
            }

            return typeName;
        }
    }
}
