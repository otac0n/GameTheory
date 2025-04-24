// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Serialization
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class PgnGame
    {
        private readonly ILookup<string, string> lookup;

        public PgnGame(IEnumerable<KeyValuePair<string, string>> tags, GameState startingPosition, IEnumerable<object> objects)
        {
            this.Tags = tags.ToList().AsReadOnly();
            this.lookup = this.Tags.ToLookup(t => t.Key, t => t.Value);
            this.StartingPosition = startingPosition;
            this.Objects = objects.ToList().AsReadOnly();
        }

        public IEnumerable<string> this[string tag] => this.lookup[tag];

        public IList<KeyValuePair<string, string>> Tags { get; }

        public GameState StartingPosition { get; }

        public IList<object> Objects { get; }
    }
}
