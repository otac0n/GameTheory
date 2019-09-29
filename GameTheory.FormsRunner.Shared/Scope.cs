// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class Scope
    {
        public Scope(string path = "", string name = null, IDictionary<string, object> properties = null)
        {
            this.Path = path ?? string.Empty;
            this.Name = name;
            this.Properties = properties?.ToImmutableDictionary();
        }

        private Scope(Scope parent, string name)
        {
            this.Parent = parent;
            this.Path = string.IsNullOrEmpty(this.Path)
                ? name
                : name.StartsWith("[")
                    ? $"{this.Path}{name}"
                    : $"{this.Path}.{name}";
            this.Name = name;
        }

        public Scope Parent { get; }

        public string Path { get; }

        public string Name { get; }

        public ImmutableDictionary<string, object> Properties { get; }

        public Scope Extend(string name) => new Scope(this, name);

        public T GetPropertyOrDefault<T>(string key, T @default = default)
        {
            if (this.Properties != null && this.Properties.TryGetValue(key, out var value) && value is T result)
            {
                return result;
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetPropertyOrDefault(key, @default);
            }

            return @default;
        }

        public static class SharedProperties
        {
            public static readonly string PlayerToken = "PlayerToken";
        }
    }
}
