// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Catalogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using GameTheory.Catalogs;
    using GameTheory.Games.Chess.Uci.Protocol;
    using Newtonsoft.Json;

    internal class UciCatalogPlayer : ICatalogPlayer, IDisposable
    {
        private static readonly Dictionary<string, Tuple<string, string>> KnownOptions = new Dictionary<string, Tuple<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Hash"] = Tuple.Create(Resources.Hash, Resources.HashDescription),
            ["NalimovPath"] = Tuple.Create(Resources.NalimovPath, Resources.NalimovPathDescription),
            ["NalimovCache"] = Tuple.Create(Resources.NalimovCache, Resources.NalimovCacheDescription),
            ["Ponder"] = Tuple.Create(Resources.Ponder, Resources.PonderDescription),
            ["OwnBook"] = Tuple.Create(Resources.OwnBook, Resources.OwnBookDescription),
            ["MultiPV"] = Tuple.Create(Resources.MultiPV, Resources.MultiPVDescription),
            ["SyzygyPath"] = Tuple.Create(Resources.SyzygyPath, Resources.SyzygyPathDescription),
            ["SyzygyProbeDepth"] = Tuple.Create(Resources.SyzygyProbeDepth, Resources.SyzygyProbeDepthDescription),
            ["Syzygy Probe Depth"] = Tuple.Create(Resources.SyzygyProbeDepth, Resources.SyzygyProbeDepthDescription),
            ["SyzygyProbeLimit"] = Tuple.Create(Resources.SyzygyProbeLimit, Resources.SyzygyProbeLimitDescription),
            ["Syzygy Probe Limit"] = Tuple.Create(Resources.SyzygyProbeLimit, Resources.SyzygyProbeLimitDescription),
            ["Syzygy50MoveRule"] = Tuple.Create(Resources.Syzygy50MoveRule, Resources.Syzygy50MoveRuleDescription),
            ["Syzygy 50 Move Rule"] = Tuple.Create(Resources.Syzygy50MoveRule, Resources.Syzygy50MoveRuleDescription),
            ["UCI_Chess960"] = Tuple.Create(Resources.UCI_Chess960, Resources.UCI_Chess960Description),
            ["UCI_ShowCurrLine"] = Tuple.Create(Resources.UCI_ShowCurrLine, Resources.UCI_ShowCurrLineDescription),
            ["UCI_ShowRefutations"] = Tuple.Create(Resources.UCI_ShowRefutations, Resources.UCI_ShowRefutationsDescription),
            ["UCI_LimitStrength"] = Tuple.Create(Resources.UCI_LimitStrength, Resources.UCI_LimitStrengthDescription),
            ["UCI_Elo"] = Tuple.Create(Resources.UCI_Elo, Resources.UCI_EloDescription),
            ["UCI_AnalyseMode"] = Tuple.Create(Resources.UCI_AnalyseMode, Resources.UCI_AnalyseModeDescription),
            ["UCI_Opponent"] = Tuple.Create(Resources.UCI_Opponent, Resources.UCI_OpponentDescription),
        };

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

        public IReadOnlyList<Initializer> Initializers => this.initializers ??= new ReadOnlyCollection<Initializer>(new[]
        {
                CreateInitializer(this.executablePath, this.engine.Value.Options),
        });

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
            var position = 1;
            var parameters = new List<Parameter>
            {
                new Parameter("playerToken", typeof(PlayerToken), default, null),
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
                var displayName = option.Name;
                string description = null;
                bool hasDefault;
                List<ValidationAttribute> attributes = null;
                void AddAttribute(ValidationAttribute attribute)
                {
                    (attributes ??= new List<ValidationAttribute>()).Add(attribute);
                }

                if (option.Name.StartsWith("Nalimov", StringComparison.OrdinalIgnoreCase) ||
                    option.Name.StartsWith("Syzygy", StringComparison.OrdinalIgnoreCase) ||
                    option.Name.StartsWith("UCI_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (KnownOptions.TryGetValue(option.Name, out var displayValues))
                {
                    displayName = displayValues.Item1;
                    description = displayValues.Item2;
                }

                if (option.Type == "check")
                {
                    var pos = position++;
                    hasDefault = bool.TryParse(option.Default, out var defaultValue);
                    parameters.Add(new Parameter(option.Name, typeof(bool), displayName, hasDefault ? defaultValue : default, description, null, false, attributes));

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
                    var hasRange = false;
                    if (int.TryParse(option.Min, out var minInt) & int.TryParse(option.Max, out var maxInt))
                    {
                        hasRange = true;
                        AddAttribute(new RangeAttribute(minInt, maxInt));
                    }

                    parameters.Add(new Parameter(option.Name, typeof(int), displayName, hasDefault ? defaultValue : default, description, null, false, attributes));

                    Apply((args, list) =>
                    {
                        var value = (int)args[pos];
                        if (!hasDefault || value != defaultValue)
                        {
                            if (hasRange)
                            {
                                ArgumentOutOfRangeException.ThrowIfLessThan(value, minInt, option.Name);
                                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, maxInt, option.Name);
                            }

                            list.Add(new SetOptionCommand(option.Name, value.ToString()));
                        }
                    });
                }
                else if (option.Type == "combo")
                {
                    var pos = position++;
                    hasDefault = option.Default is string;
                    var hasEnum = false;
                    Type @enum = null;
                    if (option.Vars.Count > 0)
                    {
                        hasEnum = true;
                        @enum = EnumCache.GetEnum(option.Vars);
                        AddAttribute(new EnumDataTypeAttribute(@enum));

                        parameters.Add(new Parameter(option.Name, @enum, displayName, hasDefault ? Enum.ToObject(@enum, hasDefault ? option.Vars.IndexOf(option.Default) : 0) : default, description, null, false, attributes));

                        Apply((args, list) =>
                        {
                            var value = args[pos].ToString();
                            if (!hasDefault || value != option.Default)
                            {
                                if (hasEnum && !option.Vars.Any(v => v == value))
                                {
                                    throw new ArgumentOutOfRangeException(option.Name);
                                }

                                list.Add(new SetOptionCommand(option.Name, value));
                            }
                        });
                    }
                    else
                    {
                        parameters.Add(new Parameter(option.Name, typeof(string), displayName, hasDefault ? option.Default : default, description, null, false, attributes));

                        Apply((args, list) =>
                        {
                            var value = (string)args[pos];
                            if (!hasDefault || value != option.Default)
                            {
                                if (hasEnum && !option.Vars.Any(v => v == value))
                                {
                                    throw new ArgumentOutOfRangeException(option.Name);
                                }

                                list.Add(new SetOptionCommand(option.Name, value));
                            }
                        });
                    }
                }
                else if (option.Type == "string")
                {
                    var pos = position++;
                    hasDefault = option.Default is string;
                    parameters.Add(new Parameter(option.Name, typeof(string), displayName, hasDefault ? option.Default : default, description, null, false, attributes));

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
                FormatUtilities.FormatList(parameters.Select(p => p.DisplayName)),
                args =>
                {
                    var commands = new List<SetOptionCommand>();
                    apply(args, commands);
                    return new UciPlayer((PlayerToken)args[0], path, null, commands);
                },
                parameters);
        }

        private static class EnumCache
        {
            private static int nextId;

            static EnumCache()
            {
                var assemblyName = new AssemblyName("UciEnums");
                var builder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                EnumCache.DynamicModule = builder.DefineDynamicModule(assemblyName.Name);
                EnumCache.Cache = new ConcurrentDictionary<string, Type>();
            }

            public static ConcurrentDictionary<string, Type> Cache { get; }

            public static ModuleBuilder DynamicModule { get; }

            public static Type GetEnum(IList<string> values)
            {
                ArgumentNullException.ThrowIfNull(values);
                ArgumentOutOfRangeException.ThrowIfZero(values.Count, nameof(values));

                var key = string.Join("|", values.Select(v => v.Replace("\\", "\\\\").Replace("|", "\\|")));
                return EnumCache.Cache.GetOrAdd(key, _ =>
                {
                    var @enum = EnumCache.DynamicModule.DefineEnum($"Enum{Interlocked.Increment(ref EnumCache.nextId)}", TypeAttributes.Public, typeof(int));
                    for (var i = 0; i < values.Count; i++)
                    {
                        @enum.DefineLiteral(values[i], i);
                    }

                    return @enum.CreateTypeInfo();
                });
            }
        }
    }
}
