﻿// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Catalogs
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A convenience class for dealing with types implementing <see cref="IGameState{TMove}"/>.
    /// </summary>
    public sealed class Game
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="gameType">The type of game to expose.</param>
        public Game(Type gameType)
            : this(gameType, GetMoveType(gameType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="gameStateType">The type of game to expose.</param>
        /// <param name="moveType">The type of moves the game supports.</param>
        /// <param name="name">The name of the game.</param>
        public Game(Type gameStateType, Type moveType, string name = null)
        {
            this.GameStateType = gameStateType ?? throw new ArgumentNullException(nameof(gameStateType));
            this.MoveType = moveType ?? throw new ArgumentNullException(nameof(moveType));
            this.Name = string.IsNullOrEmpty(name)
                ? GetGameName(this.GameStateType)
                : name;
        }

        /// <summary>
        /// Gets the type used as a game state.
        /// </summary>
        public Type GameStateType { get; }

        /// <summary>
        /// Gets the type used for moves.
        /// </summary>
        public Type MoveType { get; }

        /// <summary>
        /// Gets the name of the game.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the move type of a game state type that implements <see cref="IGameState{TMove}"/>.
        /// </summary>
        /// <param name="gameStateType">The type implementing <see cref="IGameState{TMove}"/>.</param>
        /// <returns>The type of moves supported.</returns>
        public static Type GetMoveType(Type gameStateType)
        {
            var info = gameStateType.GetTypeInfo();

            if (info.Name.Contains("GameState"))
            {
            }

            return (from i in info.ImplementedInterfaces
                    where i.IsConstructedGenericType
                    where i.GetGenericTypeDefinition() == typeof(IGameState<>)
                    select i.GetTypeInfo().GenericTypeArguments[0]).FirstOrDefault();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }

        private static string GetGameName(Type gameType)
        {
            var typeName = gameType.FullName;

            const string gameStateSuffix = "GameState";
            if (typeName.EndsWith(gameStateSuffix, StringComparison.OrdinalIgnoreCase))
            {
                typeName = typeName.Substring(0, typeName.Length - gameStateSuffix.Length);
            }

            return typeName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? gameType.FullName;
        }
    }
}