// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Serialization
{
    using System.Collections.Generic;
    using System.Linq;
    using GameTheory.GameTree;

    public sealed class PgnGame : AnnotatedVariation<GameState, Move>
    {
        private readonly ILookup<string, string> lookup;

        public PgnGame(IEnumerable<KeyValuePair<string, string>> tags, GameState startingPosition, IEnumerable<object> objects)
            : base(startingPosition, objects)
        {
            this.Tags = tags.ToList().AsReadOnly();
            this.lookup = this.Tags.ToLookup(t => t.Key, t => t.Value);
        }

        public IEnumerable<string> this[string tag] => this.lookup[tag];

        public IList<KeyValuePair<string, string>> Tags { get; }
    }
}
