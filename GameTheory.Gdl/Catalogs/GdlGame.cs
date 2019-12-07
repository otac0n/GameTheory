// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GameTheory.Catalogs;
    using Newtonsoft.Json;

    public class GdlGame : ICatalogGame, ICatalogPlayer
    {
        private readonly Lazy<Type> gameStateType;
        private readonly string gdlPath;
        private readonly Lazy<IReadOnlyList<Initializer>> initializers;
        private readonly Lazy<Type> moveType;
        private readonly Lazy<Type> playerType;

        public GdlGame(string metadataPath)
        {
            this.MetadataPath = metadataPath;
            this.Metadata = JsonConvert.DeserializeObject<GameMetadata>(File.ReadAllText(this.MetadataPath));
            this.gdlPath = Path.Combine(Path.GetDirectoryName(this.MetadataPath), this.Metadata.RuleSheet);

            this.Name = this.Metadata.GameName;
            var compileResult = new Lazy<CompileResult>(
                () =>
                {
                    var gdl = File.ReadAllText(this.gdlPath);
                    var compiler = new GameCompiler();
                    var result = compiler.Compile(gdl, this.gdlPath);
                    if (result.Errors.Where(e => !e.IsWarning).Any())
                    {
                        throw new InvalidOperationException(string.Join(Environment.NewLine, result.Errors));
                    }

                    return result;
                },
                isThreadSafe: true);
            this.gameStateType = new Lazy<Type>(
                () => compileResult.Value.Type,
                isThreadSafe: true);
            this.playerType = new Lazy<Type>(
                () => compileResult.Value.Player,
                isThreadSafe: true);
            this.moveType = new Lazy<Type>(
                () => ReflectionUtilities.GetMoveType(this.GameStateType),
                isThreadSafe: true);
            this.initializers = new Lazy<IReadOnlyList<Initializer>>(
                () => ReflectionUtilities.GetPublicInitializers(this.gameStateType.Value).ToList().AsReadOnly(),
                isThreadSafe: true);
        }

        /// <inheritdoc />
        public Type GameStateType => this.gameStateType.Value;

        public IReadOnlyList<Initializer> Initializers => this.initializers.Value;

        /// <summary>
        /// Gets the game metadata for the game.
        /// </summary>
        public GameMetadata Metadata { get; }

        /// <summary>
        /// Gets the path of the game's source metadata.
        /// </summary>
        public string MetadataPath { get; }

        /// <inheritdoc />
        public Type MoveType => this.moveType.Value;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type PlayerType => this.playerType.Value;

        public bool IsGameStateType(Type gameStateType) => this.gameStateType.IsValueCreated ? this.gameStateType.Value == gameStateType : false;

        /// <inheritdoc />
        public override string ToString() => this.Name;
    }
}
