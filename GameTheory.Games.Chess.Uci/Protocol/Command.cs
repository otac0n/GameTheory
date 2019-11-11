// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    public abstract class Command
    {
        public Command(string verb)
        {
            this.Verb = verb;
        }

        public string Verb { get; }

        public override string ToString() => this.Verb;
    }
}
