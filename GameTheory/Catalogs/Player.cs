// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Linq;

    /// <summary>
    /// A convenience class for dealing with types implementing <see cref="IPlayer{TMove}"/>.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="playerType">The type of the player.</param>
        /// <param name="moveType">The type used for moves.</param>
        /// <param name="name">The name of the player.</param>
        public Player(Type playerType, Type moveType, string name = null)
        {
            this.PlayerType = playerType ?? throw new ArgumentNullException(nameof(playerType));
            this.MoveType = moveType ?? throw new ArgumentNullException(nameof(moveType));
            this.Name = string.IsNullOrEmpty(name)
                ? GetPlayerName(this.PlayerType)
                : name;
        }

        /// <summary>
        /// Gets the type of the player.
        /// </summary>
        public Type PlayerType { get; }

        /// <summary>
        /// Gets the type used for moves.
        /// </summary>
        public Type MoveType { get; }

        /// <summary>
        /// Gets the name of the player.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }

        private static string GetPlayerName(Type playerType)
        {
            var typeName = playerType.Name.Split('`').First();

            const string playerSuffix = "Player";
            if (typeName.EndsWith(playerSuffix))
            {
                typeName = typeName.Substring(0, typeName.Length - playerSuffix.Length);
            }

            return typeName;
        }
    }
}
