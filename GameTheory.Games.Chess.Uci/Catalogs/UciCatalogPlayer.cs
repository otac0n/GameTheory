// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using GameTheory.Catalogs;
    using GameTheory.Games.Chess.Uci.Protocol;
    using Newtonsoft.Json;

    internal class UciCatalogPlayer : ICatalogPlayer, IDisposable
    {
        private readonly string executablePath;
        private Lazy<UciEngine> engine;
        private IReadOnlyList<Initializer> initializers;

        public UciCatalogPlayer(string metadataPath)
        {
            this.MetadataPath = metadataPath;
            this.Metadata = JsonConvert.DeserializeObject<EngineMetadata>(File.ReadAllText(this.MetadataPath));
            this.executablePath = Path.Combine(Path.GetDirectoryName(this.MetadataPath), this.Metadata.Executable);
            this.engine = new Lazy<UciEngine>(() =>
            {
                var engine = new UciEngine(this.executablePath, this.Metadata.Arguments);
                engine.Execute(QuitCommand.Instance);
                return engine;
            });
        }

        public Type GameStateType => typeof(GameState);

        public IReadOnlyList<Initializer> Initializers => this.initializers ?? (this.initializers = new ReadOnlyCollection<Initializer>(new[]
        {
                CreateInitializer(this.executablePath, this.engine.Value.Options),
        }));

        public EngineMetadata Metadata { get; }

        public string MetadataPath { get; }

        public Type MoveType => typeof(Move);

        public string Name => this.Metadata.EngineName ?? this.engine.Value.Name;

        public Type PlayerType => typeof(UciPlayer);

        public void Dispose()
        {
            if (this.engine != null && this.engine.IsValueCreated)
            {
                this.engine.Value.Dispose();
                this.engine = null;
            }
        }

        private static Initializer CreateInitializer(string path, IEnumerable<OptionCommand> options)
        {
            var member = typeof(UciPlayer);
            var position = 0;
            var parameters = new List<ParameterInfo>
            {
                new DynamicParameterInfo("playerToken", typeof(PlayerToken), position++, false, null, member, null),
            };

            foreach (var option in options)
            {
                object defaultValue = null;
                var hasDefault = false;
                CustomAttributeData[] attributes = null;

                switch (option.Type)
                {
                    case "check":
                        if (hasDefault = bool.TryParse(option.Default, out var defaultBool))
                        {
                            defaultValue = defaultBool;
                        }

                        parameters.Add(new DynamicParameterInfo(option.Name, typeof(bool), position++, hasDefault, defaultValue, member, attributes));
                        break;

                    case "spin":
                        if (hasDefault = int.TryParse(option.Default, out var defaultInt))
                        {
                            defaultValue = defaultInt;
                        }

                        if (int.TryParse(option.Min, out var minInt) && int.TryParse(option.Max, out var maxInt))
                        {
                            attributes = new[] { new DynamicAttributeData(typeof(RangeAttribute).GetConstructor(new[] { typeof(int), typeof(int) }), new object[] { minInt, maxInt }) };
                        }

                        parameters.Add(new DynamicParameterInfo(option.Name, typeof(int), position++, hasDefault, defaultValue, member, attributes));
                        break;

                    case "combo":
                        parameters.Add(new DynamicParameterInfo(option.Name, typeof(string), position++, option.Default is string, option.Default, member, attributes));
                        break;

                    case "string":
                        if (option.Min != null && option.Max != null)
                        {
                            attributes = new[] { new DynamicAttributeData(typeof(RangeAttribute).GetConstructor(new[] { typeof(Type), typeof(string), typeof(string) }), new object[] { typeof(string), option.Min, option.Max }) };
                        }

                        parameters.Add(new DynamicParameterInfo(option.Name, typeof(string), position++, option.Default is string, option.Default, member, attributes));
                        break;

                    default:
                        continue;
                }
            }

            return new Initializer(FormatUtilities.FormatList(parameters.Select(p => p.Name)), args => new UciPlayer((PlayerToken)args[0], path, null, null), parameters);
        }
    }
}
