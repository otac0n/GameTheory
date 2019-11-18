// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "ucinewgame" command.
    /// </summary>
    public class UciNewGameCommand : Command
    {
        private UciNewGameCommand()
            : base("ucinewgame")
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="UciNewGameCommand"/>.
        /// </summary>
        public static UciNewGameCommand Instance { get; } = new UciNewGameCommand();
    }
}
