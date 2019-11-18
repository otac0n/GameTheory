// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The base class for all UCI commands.
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="verb">The command verb.</param>
        public Command(string verb)
        {
            this.Verb = verb;
        }

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Verb { get; }

        /// <inheritdoc/>
        public override string ToString() => this.Verb;
    }
}
