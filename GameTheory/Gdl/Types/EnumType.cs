// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl.Types
{
    using System.Collections;
    using System.Collections.Generic;

    public class EnumType : ObjectType, IEnumerable<ObjectInfo>
    {
        public EnumType(string name)
            : base(name)
        {
        }

        public HashSet<ObjectInfo> Objects { get; }

        public void Add(ObjectInfo objectInfo)
        {
            this.Objects.Add(objectInfo);
        }

        /// <inheritdoc/>
        public IEnumerator<ObjectInfo> GetEnumerator() => this.Objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
