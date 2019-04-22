// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using GameTheory.Gdl.Types;
    using KnowledgeInterchangeFormat.Expressions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Flags]
    public enum ScopeFlags
    {
        Public = 0b01,
        Private = 0b10,
    }

    public class Scope<TKey>
    {
        private static readonly Regex NameSplitter = new Regex(@"(?<=\p{Ll})(?=[0-9\p{Lu}])|(?<=\p{Lu})(?=\p{Lu}\p{Ll}|[0-9])|(?<=[0-9])(?=[\p{Ll}\p{Lu}])|[^0-9\p{Lu}\p{Ll}]+", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly ImmutableHashSet<string> names;
        private readonly ImmutableDictionary<TKey, (string @public, string @private)> value;

        public Scope()
        {
            this.names = ImmutableHashSet<string>.Empty;
            this.value = ImmutableDictionary<TKey, (string, string)>.Empty;
        }

        private Scope(ImmutableHashSet<string> names, ImmutableDictionary<TKey, (string, string)> value)
        {
            this.names = names;
            this.value = value;
        }

        public string TryGetPublic(TKey key) => this.value.TryGetValue(key, out var found) ? found.@public : null;

        public string TryGetPrivate(TKey key) => this.value.TryGetValue(key, out var found) ? found.@private : null;

        public bool ContainsKey(TKey key) => this.value.ContainsKey(key);

        public Scope<TKey> AddPrivate(TKey key, params string[] nameHints) => this.Add(key, ScopeFlags.Private, nameHints);

        public Scope<TKey> SetPrivate(TKey key, string path) =>
            new Scope<TKey>(
                this.names.Add(path),
                this.value.Add(key, (null, path)));

        public Scope<TKey> Add(TKey key, ScopeFlags flags, params string[] nameHints)
        {
            (string, string)? value = null;

            var normalizedHints = Array.ConvertAll(nameHints, hint => NormalizeName(hint, CultureInfo.CurrentCulture));
            foreach (var hint in normalizedHints)
            {
                if (hint != null &&
                    ((flags & ScopeFlags.Public) == 0 || !this.names.Contains(hint.Value.@public)) &&
                    ((flags & ScopeFlags.Private) == 0 || !this.names.Contains(hint.Value.@private)))
                {
                    value = (
                        (flags & ScopeFlags.Public) == 0 ? null : hint.Value.@public,
                        (flags & ScopeFlags.Private) == 0 ? null : hint.Value.@private);
                    break;
                }
            }

            if (value == null)
            {
                for (var i = 1; i <= 100; i++)
                {
                    var @public = $"Value{i}";
                    var @private = $"value{i}";
                    if (((flags & ScopeFlags.Public) == 0 || !this.names.Contains(@public)) &&
                        ((flags & ScopeFlags.Private) == 0 || !this.names.Contains(@private)))
                    {
                        value = (@public, @private);
                        break;
                    }
                }

                if (value == null)
                {
                    throw new InvalidOperationException();
                }
            }

            var names = this.names;

            if ((flags & ScopeFlags.Public) == 0)
            {
                names = names.Add(value.Value.Item1);
            }

            if ((flags & ScopeFlags.Private) == 0)
            {
                names = names.Add(value.Value.Item2);
            }

            return new Scope<TKey>(
                names,
                this.value.SetItem(key, value.Value));
        }

        private static (string @public, string @private)? NormalizeName(string hint, CultureInfo culture)
        {
            var @public = string.Concat(
                NameSplitter.Split(hint ?? string.Empty)
                    .Where(part => !string.IsNullOrEmpty(part))
                    .Select(part => char.ToUpper(part[0], culture) + part.Substring(1).ToLower(culture)));

            if (string.IsNullOrEmpty(@public))
            {
                return null;
            }
            else if (char.IsUpper(@public[0]))
            {
                return (@public, char.ToLower(@public[0], culture) + @public.Substring(1));
            }
            else
            {
                return ("_" + @public, "__" + @public);
            }
        }
    }
}
