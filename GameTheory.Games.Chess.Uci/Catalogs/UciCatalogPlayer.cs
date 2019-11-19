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

            Action<object[], List<SetOptionCommand>> apply = (args, list) => { };
            void Apply(Action<object[], List<SetOptionCommand>> action)
            {
                var prev = apply;
                apply = (args, list) =>
                {
                    prev(args, list);
                    action(args, list);
                };
            }

            foreach (var option in options)
            {
                bool hasDefault;
                List<CustomAttributeData> attributes = null;
                void AddAttribute(CustomAttributeData attribute)
                {
                    (attributes ?? (attributes = new List<CustomAttributeData>())).Add(attribute);
                }

                if (option.Type == "check")
                {
                    var pos = position++;
                    hasDefault = bool.TryParse(option.Default, out var defaultValue);
                    parameters.Add(new DynamicParameterInfo(option.Name, typeof(bool), pos, hasDefault, defaultValue, member, attributes?.ToArray()));

                    Apply((args, list) =>
                    {
                        var value = (bool)args[pos];
                        if (!hasDefault || value != defaultValue)
                        {
                            list.Add(new SetOptionCommand(option.Name, value ? "true" : "false"));
                        }
                    });
                }
                else if (option.Type == "spin")
                {
                    var pos = position++;
                    hasDefault = int.TryParse(option.Default, out var defaultValue);
                    if (int.TryParse(option.Min, out var minInt) && int.TryParse(option.Max, out var maxInt))
                    {
                        AddAttribute(ReflectionUtilities.AttributeData<RangeAttribute>(minInt, maxInt));
                    }

                    parameters.Add(new DynamicParameterInfo(option.Name, typeof(int), pos, hasDefault, defaultValue, member, attributes?.ToArray()));

                    Apply((args, list) =>
                    {
                        var value = (int)args[pos];
                        if (!hasDefault || value != defaultValue)
                        {
                            list.Add(new SetOptionCommand(option.Name, value.ToString()));
                        }
                    });
                }
                else if (option.Type == "combo")
                {
                    var pos = position++;
                    hasDefault = option.Default is string;
                    parameters.Add(new DynamicParameterInfo(option.Name, typeof(string), pos, hasDefault, option.Default, member, attributes?.ToArray()));

                    Apply((args, list) =>
                    {
                        var value = (string)args[pos];
                        if (!hasDefault || value != option.Default)
                        {
                            list.Add(new SetOptionCommand(option.Name, value));
                        }
                    });
                }
                else if (option.Type == "string")
                {
                    var pos = position++;
                    hasDefault = option.Default is string;
                    parameters.Add(new DynamicParameterInfo(option.Name, typeof(string), pos, option.Default is string, option.Default, member, attributes?.ToArray()));

                    Apply((args, list) =>
                    {
                        var value = (string)args[pos];
                        if (!hasDefault || value != option.Default)
                        {
                            list.Add(new SetOptionCommand(option.Name, value));
                        }
                    });
                }
            }

            return new Initializer(
                FormatUtilities.FormatList(parameters.Select(p => p.Name)),
                args =>
                {
                    var commands = new List<SetOptionCommand>();
                    apply(args, commands);
                    return new UciPlayer((PlayerToken)args[0], path, null, commands);
                },
                parameters);
        }
    }
}
