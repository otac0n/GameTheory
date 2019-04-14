// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;

    internal abstract class CompilePass
    {
        public abstract IList<string> BlockedByErrors { get; }

        public abstract IList<string> ErrorsProduced { get; }

        public abstract void Run(CompileResult result);
    }
}
