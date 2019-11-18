// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Protocol
{
    /// <summary>
    /// The "id" command.
    /// </summary>
    public class IdCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdCommand"/> class.
        /// </summary>
        /// <param name="field">The ID field.</param>
        /// <param name="value">The field value.</param>
        public IdCommand(string field, string value)
            : base("id")
        {
            this.Field = field;
            this.Value = value;
        }

        /// <summary>
        /// Gets the ID field.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc/>
        public override string ToString() => $"id {this.Field} {this.Value}";
    }
}
