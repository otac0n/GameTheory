// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "registration" command.
    /// </summary>
    public class RegistrationCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationCommand"/> class.
        /// </summary>
        /// <param name="status">The registration status.</param>
        public RegistrationCommand(string status)
            : base("registration")
        {
            this.Status = status;
        }

        /// <summary>
        /// Gets the registration status.
        /// </summary>
        public string Status { get; }

        /// <inheritdoc/>
        public override string ToString() => $"registration {this.Status}";
    }
}
