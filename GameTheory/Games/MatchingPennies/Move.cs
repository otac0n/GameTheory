// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.MatchingPennies
{
    /// <summary>
    /// Represents a move in Matching Pennies.
    /// </summary>
    public class Move : IMove
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="playerToken">The player who may make this move.</param>
        /// <param name="heads">A value indicating whether or not the player has chosen heads.</param>
        public Move(PlayerToken playerToken, bool heads)
        {
            this.PlayerToken = playerToken;
            this.Heads = heads;
        }

        /// <inheritdoc />
        public PlayerToken PlayerToken { get; }

        /// <summary>
        /// Gets a value indicating whether or not the player has chosen heads.
        /// </summary>
        public bool Heads { get; }

        /// <inheritdoc />
        public override string ToString() => this.Heads ? "H" : "T";
    }
}
