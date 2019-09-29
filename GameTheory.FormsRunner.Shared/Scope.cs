// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.FormsRunner.Shared
{
    public class Scope
    {
        public Scope(string path = "", string name = null)
        {
            this.Path = path ?? string.Empty;
            this.Name = name;
        }

        public string Path { get; }

        public string Name { get; }

        public Scope Extend(string name) =>
            new Scope(
                string.IsNullOrEmpty(this.Path)
                    ? name
                    : name.StartsWith("[")
                        ? $"{this.Path}{name}"
                        : $"{this.Path}.{name}",
                name);
    }
}
