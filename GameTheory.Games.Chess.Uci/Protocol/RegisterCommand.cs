// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The "register" command.
    /// </summary>
    public class RegisterCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the user to register.</param>
        /// <param name="code">The registration code to include.</param>
        public RegisterCommand([Optional] string name, [Optional] string code)
            : base("register")
        {
            this.Name = name;
            this.Code = code;
        }

        /// <summary>
        /// Gets an instance of the <see cref="RegisterCommand"/> requesting delayed registration.
        /// </summary>
        public static RegisterCommand Later { get; } = new RegisterCommand(null, null);

        /// <summary>
        /// Gets the registration code to include.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the name of the user to register.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc/>
        public override string ToString() =>
            string.IsNullOrEmpty(this.Name)
                ? string.IsNullOrEmpty(this.Code)
                    ? "register later"
                    : $"register code {this.Code}"
                : string.IsNullOrEmpty(this.Code)
                    ? $"register name {this.Name}"
                    : $"register name {this.Name} code {this.Code}";
    }
}
