// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "copyprotection" command.
    /// </summary>
    public class CopyProtectionCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyProtectionCommand"/> class.
        /// </summary>
        /// <param name="status">The copy protection status.</param>
        public CopyProtectionCommand(string status)
            : base("copyprotection")
        {
            this.Status = status;
        }

        /// <summary>
        /// Gets the copy protection status.
        /// </summary>
        public string Status { get; }

        /// <inheritdoc/>
        public override string ToString() => $"copyprotection {this.Status}";
    }
}
